using NMLServer.Lexing;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer;

using TokenInfo = (int index, int offset);

internal partial class Document
{
    public void UpdateFrom(DidChangeTextDocumentParams parameters)
    {
        if (parameters.TextDocument.Version is not { } nextVersion
            || nextVersion <= _version)
        {
            return;
        }
        foreach (var change in parameters.ContentChanges)
        {
            var text = change.Text;
            if (change.Range is { } replacedRange)
            {
                ProcessTextUpdateDelta(replacedRange, text);
                continue;
            }
            _symbolComparer.context = _source = text;
            MakeTokensFromScratch();
            MakeStatementsFromScratch();
            MakeDefinitionsFromScratch();
        }
        _version = nextVersion;
    }

    private void ProcessTextUpdateDelta(Range replacedRange, string text)
    {
        var range = ProtocolToLocal(replacedRange);
        {
            StringView span = source;
            source = string.Concat(span[..range.start], text, span[range.end..]);
        }

        FindTokensAtRangeBounds(range, out var firstBeforeRange, out var firstAfterRange);

        // Shift tokens after the range
        var shift = text.Length - (range.end - range.start);
        Parallel.For(firstAfterRange.index, _tokens.Count, i =>
        {
            _tokens[i].start += shift;
        });

        firstBeforeRange.offset = int.Max(firstBeforeRange.offset, 0);
        UpdateTokens(firstBeforeRange, firstAfterRange, shift);

        // TODO Reconstruct statement tree; replace unexpected tokens;
        MakeStatementsFromScratch();
        MakeDefinitionsFromScratch();
    }

    private (int start, int end) ProtocolToLocal(Range range)
    {
        var start = 0;
        int line = 0;
        for (int startLine = range.Start.Line; line < startLine; ++line)
        {
            start += _lineLengths[line];
        }
        var end = start;
        for (int endLine = range.End.Line; line < endLine; ++line)
        {
            end += _lineLengths[line];
        }
        return (start + range.Start.Character, end + range.End.Character);
    }

    /// <summary>
    /// Find last token to the left of the range and first to the right of it, but both not directly adjacent to it.
    /// </summary>
    /// <param name="range">The range describing replaced text chunk coordinates.</param>
    /// <param name="firstBeforeRange">
    /// The tuple (index in token list, start offset), describing last token before the range but not adjacent to it.
    /// If such token is not present, the index is set to -1 and the offset value should be discarded.
    /// </param>
    /// <param name="firstAfterRange">
    /// The tuple (index in token list, start offset), describing first token after the range but not adjacent to it.
    /// If such token is not present, the index is set to token list size and the offset value should be discarded.
    /// </param>
    private void FindTokensAtRangeBounds((int start, int end) range, out TokenInfo firstBeforeRange,
        out TokenInfo firstAfterRange)
    {
        // Replacing tokens
        //   To replace tokens, we first find the last token before and not touching replaced range.
        // We use its end position as a start for Lexer run.
        var maxTokenPos = _tokens.Count - 1;
        firstBeforeRange = (-1, 0);
        for (int left = 0, right = maxTokenPos; left <= right;)
        {
            var mid = left + (right - left) / 2;
            var current = _tokens[mid];
            var start = current.start;
            if (current.end < range.start)
            {
                // We only remember position of rightmost token to the left side of range.
                firstBeforeRange = (mid, start);
                left = mid + 1;
                continue;
            }
            if (start > range.end)
            {
                // Ignore position after range. If the range does not intersect with tokens, the last one before it
                // would be captured earlier. If no token is to the left of the range, index will be left with valid -1.
                right = mid - 1;
                continue;
            }
            if (mid == 0)
            {
                firstBeforeRange = (-1, -1);
                break;
            }
            right = mid - 1;
            for (var prev = _tokens[right]; prev.end < range.start;)
            {
                firstBeforeRange = (right, prev.start);
                break;
            }
        }

        // We also find the first token after and not touching replaced range. Its position will be
        // used to decide whether Lexer should run until the end of source or check for stopping condition
        // by comparing current token position with previously lexed ones.
        firstAfterRange = (_tokens.Count, -1);
        for (int left = firstBeforeRange.index + 1, right = maxTokenPos; left <= right;)
        {
            var mid = left + (right - left) / 2;
            var current = _tokens[mid];
            if (current.end < range.start)
            {
                // Similar situation: now we only record rightmost tokens to the right of the range.
                left = mid + 1;
                continue;
            }
            var start = current.start;
            if (start > range.end)
            {
                firstAfterRange = (mid, start);
                right = mid - 1;
                continue;
            }
            if (mid == maxTokenPos)
            {
                firstAfterRange = (mid + 1, -1);
                break;
            }
            left = mid + 1;
            for (var nextStart = _tokens[left].start; nextStart > range.end;)
            {
                firstAfterRange = (left, nextStart);
                break;
            }
        }
    }

    private void UpdateTokens(TokenInfo firstTokenToReplace, TokenInfo firstTokenAfterRange, int shift)
    {
        PositionConverter converter = new(_lineLengths);
        List<Token> lexedTokens = [];
        List<int> lexedLineLengths = [];
        StringView input = source;

        var (startLine, startChar) = converter.LocalToProtocol(firstTokenToReplace.offset);
        Lexer lexer = new(input, in lexedLineLengths, firstTokenToReplace.offset, input.Length, startChar);

        // Short-circuit if replaced text chunk is at the end of source.
        if (firstTokenAfterRange.index == _tokens.Count)
        {
            lexer.ProcessUntilFileEnd(in lexedTokens);
            goto label_AfterReachedEOF;
        }

        Token? token;
        int tokenStart;
        do
        {
            token = lexer.LexToken(out tokenStart);
            if (token is null || tokenStart >= firstTokenAfterRange.offset)
            {
                break;
            }
            lexedTokens.Add(token);
        }
        while (true);

        // Token can be null in case last lexed token consumed last existing one before as its own part. An example:
        // * range end ↓    ↓ EOF
        // * . . .    123abc -> (num, 123) A; (id, 'abc') B;
        // * . . .   a123abc -> (id, 'a123abc') C;
        // In this example C.start < B.start, but EOF was reached.
        // Thus we copy our behaviour from when range was at the end of source code.
        if (token is null)
        {
            goto label_AfterReachedEOF;
        }

        // Until we run out of old tokens, compare to current old token to check whether we are repeating previous lexing result
        for (var oldTokenIndex = firstTokenAfterRange.index; oldTokenIndex < _tokens.Count; ++oldTokenIndex)
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
            // If overshot, increase index of token compared with. We do not check the type equality, because it's
            // impossible for two tokens with different types to have the same start offset.
            if (tokenStart > currentComparedTokenStart)
            {
                continue;
            }
            // Add current piece of parsed line to the list
            lexer.Complete();
            // When replacing line lengths range, we account for said lengths being valid only for previous positions
            // of tokens, so token start must be un-shifted for finding actual position
            var (endLine, character) = converter.LocalToProtocol(token.end - shift);
            int endLineLength = _lineLengths[endLine];
            // add remaining line length, if any; first line is already accounting line length, see Lexer.ctor
            lexedLineLengths[^1] += endLineLength - character;

            _lineLengths.ReplaceRange(lexedLineLengths, startLine, endLine + 1);
            if (firstTokenToReplace.index <= 0)
            {
                _tokens.ComplementStartByRange(in lexedTokens, oldTokenIndex);
            }
            else
            {
                _tokens.ReplaceRange(lexedTokens, firstTokenToReplace.index, oldTokenIndex);
            }
            return;
        }
        // If reached end of old token list, lex until EOF and complement the end part.
        lexer.ProcessUntilFileEnd(in lexedTokens);

        label_AfterReachedEOF:
        _lineLengths.ComplementEndByRange(in lexedLineLengths, startLine);
        if (firstTokenToReplace.index <= 0)
        {
            _tokens = lexedTokens;
        }
        else
        {
            _tokens.ComplementEndByRange(in lexedTokens, firstTokenToReplace.index);
        }
    }
}