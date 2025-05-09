using EmmyLua.LanguageServer.Framework.Protocol.Message.TextDocument;
using NMLServer.Extensions;
using NMLServer.Model.Lexis;

namespace NMLServer.Model;

using TokenInfo = (int index, int offset);
using RangeInfo = (int start, int end);

internal partial class Document
{
    public void UpdateFrom(DidChangeTextDocumentParams request)
    {
        int newVersion = request.TextDocument.Version;
        if (newVersion <= version)
        {
            return;
        }
        foreach (var change in request.ContentChanges)
        {
            var text = change.Text;
            if (change.Range is { } replacedRange)
            {
                ProcessTextUpdateDelta(replacedRange, text);
                continue;
            }
            source = text;
            MakeTokensFromScratch();
            MakeStatementsFromScratch();
        }
        MakeDefinitionsFromScratch();
        version = newVersion;
    }

    private void ProcessTextUpdateDelta(Range replacedRange, string text)
    {
        var range = ProtocolToLocal(replacedRange);
        // Shift for tokens after the range
        var shift = text.Length - (range.end - range.start);

        UpdateSource(range, text);
        UpdateTokens(range, shift);
        MakeStatementsFromScratch();
    }

    private void UpdateSource(RangeInfo range, string text)
    {
        StringView span = source;
        source = string.Concat(span[..range.start], text, span[range.end..]);
    }

    private RangeInfo ProtocolToLocal(Range range)
    {
        var start = 0;
        var line = 0;
        for (var startLine = range.Start.Line; line < startLine; ++line)
            start += _lineLengths[line];

        var end = start;
        for (var endLine = range.End.Line; line < endLine; ++line)
            end += _lineLengths[line];

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
    private void FindTokensAtRangeBounds(RangeInfo range, out TokenInfo firstBeforeRange,
        out TokenInfo firstAfterRange)
    {
        // To replace tokens, we first find the last token before and not touching replaced range.
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
            var nextStart = _tokens[left].start;
            if (nextStart > range.end)
            {
                firstAfterRange = (left, nextStart);
            }
        }
    }

    private void UpdateTokens(RangeInfo range, int shift)
    {
        FindTokensAtRangeBounds(range, out var firstTokenToReplace, out var firstTokenAfterRange);

        // Shift tokens after the range
        for (int i = firstTokenAfterRange.index, max = _tokens.Count; i < max; ++i)
        {
            _tokens[i].start += shift;
        }
        firstTokenToReplace.offset = int.Max(firstTokenToReplace.offset, 0);

        var converter = MakeConverter();
        List<Token> lexedTokens = [];
        List<int> lexedLineLengths = [];

        var startOffset = converter.LocalToProtocol(firstTokenToReplace.offset);
        Lexer lexer = new(source, lexedLineLengths, firstTokenToReplace.offset, startOffset.Character);

        // Short-circuit if replaced text chunk is at the end of source.
        if (firstTokenAfterRange.index == _tokens.Count)
        {
            lexer.ProcessUntilFileEnd(in lexedTokens);
            goto label_ReachedEOF;
        }

        Token? token;
        int tokenStart;
        do
        {
            token = lexer.LexToken(out tokenStart);
            // Token can be null in case last lexed token consumed last existing one before as its own part. An example:
            // * range end ↓    ↓ EOF
            // * . . .    123abc -> (num, 123) A; (id, 'abc') B;
            // * . . .   a123abc -> (id, 'a123abc') C;
            // In this example C.start < B.start, but EOF was reached. We therefore copy the behaviour from when range was at the end of source code.
            if (token is null)
            {
                goto label_ReachedEOF;
            }
            if (tokenStart >= firstTokenAfterRange.offset)
            {
                break;
            }
            lexedTokens.Add(token);
        }
        while (true);

        // Until we run out of old tokens, compare to current old token to check if we are repeating previous result
        for (var oldTokenIndex = firstTokenAfterRange.index; oldTokenIndex < _tokens.Count; ++oldTokenIndex)
        {
            var oldToken = _tokens[oldTokenIndex];
            var oldStart = oldToken.start;
            // Lex tokens while any are left and last lexed token is 'behind' existing one.
            while (tokenStart < oldStart)
            {
                lexedTokens.Add(token);
                token = lexer.LexToken(out tokenStart);
                if (token is null)
                {
                    goto label_ReachedEOF;
                }
            }
            // If overshot, increase index of token compared with. We do not check the type equality, because it's
            // impossible for two tokens with different types to have the same offset.
            if (tokenStart > oldStart)
            {
                continue;
            }
            // Add current piece of parsed line to the list of lengths.
            lexer.CompleteLine();

            // Add remaining line length (first line was shifted in by Lexer already).
            // We use old lengths for calculations, so token.end must be corrected.
            var (endLine, character) = converter.LocalToProtocol(token.end - shift);
            lexedLineLengths[^1] += _lineLengths[endLine] - character;
            _lineLengths.ReplaceRange(lexedLineLengths, startOffset.Line, endLine + 1);
            if (firstTokenToReplace.index < 1)
                _tokens.ComplementStartByRange(in lexedTokens, oldTokenIndex);
            else
                _tokens.ReplaceRange(lexedTokens, firstTokenToReplace.index, oldTokenIndex);
            return;
        }

        // If reached end of old token list, run lexer until EOF, then complement the end part.
        lexer.ProcessUntilFileEnd(in lexedTokens);

        label_ReachedEOF:
        _lineLengths.ComplementEndByRange(in lexedLineLengths, startOffset.Line);
        // do not operate with spans if simple list replacement is enough
        if (firstTokenToReplace.index < 1)
            _tokens = lexedTokens;
        else
            _tokens.ComplementEndByRange(in lexedTokens, firstTokenToReplace.index);
    }
}