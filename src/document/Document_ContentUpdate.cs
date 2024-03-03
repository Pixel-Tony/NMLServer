using NMLServer.Lexing;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer;

using IndexedToken = (int pos, Token? token);

internal partial class Document
{
    public void UpdateFrom(DidChangeTextDocumentParams parameters)
    {
        int? version = parameters.TextDocument.Version;
        if (version is not { } nextVersion || _version >= nextVersion)
        {
            return;
        }
        _version = nextVersion;
        foreach (var change in parameters.ContentChanges)
        {
            var text = change.Text;
            if (change.Range is { } replacedRange)
            {
                var range = ProtocolToLocal(replacedRange);
                ArgumentOutOfRangeException.ThrowIfNegative(range.start);
                ArgumentOutOfRangeException.ThrowIfLessThan(range.end, range.start);
                ProcessTextUpdateDelta(range, text);
                continue;
            }
            _symbolComparer.context = _source = text;
            MakeTokens();
        }
        _unexpectedTokens.Clear();
        MakeStatements();
        MakeDefinitions();

        _isActualVersionOfDiagnostics = false;
    }

    private void ProcessTextUpdateDelta((int start, int end) range, string text)
    {
        // Replacing source span
        var span = _source.AsSpan();
        span = _symbolComparer.context = _source = string.Concat(span[..range.start], text, span[range.end..]);

        int shift = text.Length - (range.end - range.start);

        FindBoundingTokensForRange(range, out var firstBeforeRange, out var firstAfterRange);

        // Shift tokens after the range
        Parallel.For(firstAfterRange.pos, _tokens.Count, i => _tokens[i].start += shift);

        int lexStart = firstBeforeRange.pos >= 0
            ? firstBeforeRange.token!.start
            : 0;
        int firstTokenToReplace = firstBeforeRange.pos;

        ComplementTokensLines(span, lexStart, firstTokenToReplace, firstAfterRange, shift);

        // TODO Reconstruct statement AST; Rescan for semantic tokens; Replace diagnostics; Replace unexpected tokens
    }

    private void FindBoundingTokensForRange((int start, int end) range,
        out IndexedToken firstBeforeRange, out IndexedToken firstAfterRange)
    {
        // TODO only token positions are used, replace tokens in return values with said positions
        // Replacing tokens
        //   To replace tokens, we first find the last token before and not touching replaced range.
        // We use its end position as a start for Lexer run.
        int maxTokenPos = _tokens.Count - 1;
        firstBeforeRange = (-1, null);
        for (int left = 0, right = maxTokenPos; left <= right;)
        {
            int mid = left + (right - left) / 2;
            var current = _tokens[mid];
            if (current.end < range.start)
            {
                // We only remember position of rightmost token to the left side of range.
                firstBeforeRange = (mid, current);
                left = mid + 1;
                continue;
            }
            if (current.start > range.end)
            {
                // Ignore position after range. If the range does not intersect with tokens, the last one before it
                // would be captured earlier. If no token is to the left of the range, index will be left with valid -1.
                right = mid - 1;
                continue;
            }
            if (mid == 0)
            {
                firstBeforeRange = (-1, null);
                break;
            }
            for (var prev = _tokens[--mid]; prev.end < range.start;)
            {
                firstBeforeRange = (mid, prev);
                break;
            }
            right = mid;
        }

        // We also find the first token after and not touching replaced range. Its position will be
        // used to decide whether Lexer should run until the end of source or check for stopping condition
        // by comparing current token position with previously lexed ones.
        // Todo: only integer start/end prop used, replace tuple with prop value or -1
        firstAfterRange = (_tokens.Count, null);
        for (int left = firstBeforeRange.pos + 1, right = maxTokenPos; left <= right;)
        {
            int mid = left + (right - left) / 2;
            var current = _tokens[mid];
            if (current.end < range.start)
            {
                // Similar situation: now we only record rightmost tokens to the right of the range.
                left = mid + 1;
                continue;
            }
            if (current.start > range.end)
            {
                firstAfterRange = (mid, current);
                right = mid - 1;
                continue;
            }
            if (mid == maxTokenPos)
            {
                firstAfterRange = (mid + 1, null);
                break;
            }
            for (var next = _tokens[mid + 1]; next.start > range.end;)
            {
                firstAfterRange = (mid + 1, next);
                break;
            }
            left = mid + 1;
        }
    }

    private void ComplementTokensLines(StringView input, int start, int firstTokenToReplaceIndex,
        IndexedToken firstAfterRange, int shift)
    {
        var (startLine, startChar) = LocalToProtocol(start);
        List<Token> lexedTokens = [];
        List<int> lexedLineLengths = [];

        Lexer lexer = new(input, in lexedLineLengths, start, input.Length, startChar);

        // Decide whether to parse until end
        if (firstAfterRange.pos == _tokens.Count)
        {
            lexer.ProcessUntilFileEnd(in lexedTokens);
            goto label_AfterReachedEOF;
        }

        Token? token;
        int tokenStart;
        int firstAfterRangeStart = firstAfterRange.token!.start;
        do
        {
            token = lexer.LexToken(out tokenStart);
            if (token is null || tokenStart >= firstAfterRangeStart)
            {
                break;
            }
            lexedTokens.Add(token);
        }
        while (true);

        // Token can be null in case last lexed token consumed last existing one before as its own part.
        // Here's an example:
        // *        ↓ range.end here
        // * ...  123abc <EOF> -> (number A, 123); (identifier B, 'abc');
        // * ... a123abc <EOF> -> (identifier C, 'a123abc');
        // In this example C.start < B.start, but EOF was reached nonetheless.
        // Thus we copy our behaviour from when range was at the end of source code.
        if (token is null)
        {
            goto label_AfterReachedEOF;
        }

        // Until we run out of old tokens, compare to current old token to check
        // whether we are repeating previous lexing result
        for (var oldTokenIndex = firstAfterRange.pos; oldTokenIndex < _tokens.Count; ++oldTokenIndex)
        {
            var currentComparedToken = _tokens[oldTokenIndex];
            var currentComparedTokenStart = currentComparedToken.start;

            // Lex tokens while any are left and last lexed token is 'behind' existing one
            while (tokenStart < currentComparedTokenStart)
            {
                lexedTokens.Add(token);
                token = lexer.LexToken(out tokenStart);
                if (token is null)
                {
                    goto label_AfterReachedEOF;
                }
            }
            // If overshot, increase index of token compared with. We do not check the types equality, because it is
            // impossible for two tokens with different types to start at one location of span.
            if (tokenStart > currentComparedTokenStart)
            {
                continue;
            }

            // Add current piece of parsed line to the list
            lexer.Complete();

            // When replacing line lengths range, we account for said lengths being valid only for previous positions
            // of tokens, so token start must be un-shifted for finding actual position
            var (endLine, character) = LocalToProtocol(token.end - shift);
            int endLineLength = _lineLengths[endLine];

            // add remaining line length, if any; first line is already accounting line length, see Lexer.ctor
            lexedLineLengths[^1] += endLineLength - character;

            _lineLengths.ReplaceRange(lexedLineLengths, startLine, endLine + 1);
            if (firstTokenToReplaceIndex < 0)
            {
                _tokens.ComplementStartByRange(in lexedTokens, oldTokenIndex);
            }
            else
            {
                _tokens.ReplaceRange(lexedTokens, firstTokenToReplaceIndex, oldTokenIndex);
            }
            return;
        }
        // If reached end of old token list, lex until EOF and complement as in first case
        lexer.ProcessUntilFileEnd(ref token, in lexedTokens);

        label_AfterReachedEOF:
        _lineLengths.ComplementEndByRange(in lexedLineLengths, startLine);
        if (firstTokenToReplaceIndex <= 0)
        {
            _tokens = lexedTokens;
        }
        else
        {
            _tokens.ComplementEndByRange(in lexedTokens, firstTokenToReplaceIndex);
        }
    }

    // TODO convert to ref struct method with 'rolling' current offset/line number/char number
    /// <summary>
    /// Convert start position of token to position in file.
    /// </summary>
    /// <param name="offset">The index of first token symbol.</param>
    /// <returns>The two-coordinate representation of passed source position.</returns>
    private (int line, int character) LocalToProtocol(int offset)
    {
        int i = 0;
        for (int length = _lineLengths[0]; offset > length; length = _lineLengths[++i])
        {
            offset -= length;
        }
        return (i, offset);
    }

    // TODO convert to ref struct method with 'rolling' current start/line number
    /// <summary>
    /// For given token, get sequence of ranges in form (line, startCharacter, length), that cover it.
    /// </summary>
    /// <param name="start">The index of first symbol of the token.</param>
    /// <param name="length">The length of the token.</param>
    private IEnumerable<(int line, int @char, int length)> LocalToProtocol(int start, int length)
    {
        /* Getting coordinates of starting position */
        int line = 0;
        int lineLength = _lineLengths[line];
        while (start >= lineLength)
        {
            start -= lineLength;
            lineLength = _lineLengths[++line];
        }
        var startLine = line;
        /* length now contains shift from first position line's character index */
        length += start;
        /* If on the same line - return only one coordinates */
        if (length <= lineLength)
        {
            return new[] { (line, start, length - start) };
        }
        /* Otherwise - return 'end piece' of first line, whole middle lines and 'start piece' of last line.
         * Getting coordinates of ending position
         */
        while (length >= lineLength)
        {
            length -= lineLength;
            lineLength = _lineLengths[++line];
        }
        var result = new (int, int, int)[line - startLine + 1];

        result[0] = (startLine, start, _lineLengths[startLine] - start);
        for (int i = 1, lineNum = startLine + 1; lineNum < line; ++lineNum)
        {
            result[i++] = (lineNum, 0, _lineLengths[lineNum]);
        }
        result[line] = (line, 0, length);
        return result;
    }

    private int ProtocolToLocal(Position position)
    {
        var start = position.Character;
        for (int i = 0; i < position.Line; ++i)
        {
            start += _lineLengths[i];
        }
        return start;
    }

    private (int start, int end) ProtocolToLocal(Range range)
    {
        var start = 0;
        int i = 0;
        for (int startLine = range.Start.Line; i < startLine; ++i)
        {
            start += _lineLengths[i];
        }
        var end = start;
        for (int endLine = range.End.Line; i < endLine; ++i)
        {
            end += _lineLengths[i];
        }
        return (start + range.Start.Character, end + range.End.Character);
    }
}