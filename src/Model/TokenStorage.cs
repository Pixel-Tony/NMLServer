using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using NMLServer.Extensions;
using NMLServer.Model.Lexis;

namespace NMLServer.Model;

using TokenInfo = (int index, int offset);
using RangeInfo = (int start, int end);

internal partial struct TokenStorage
{
    public TokenStorage(string initialSource)
    {
        Source = initialSource;
        Items = [];
        Lexer lexer = new(Source);
        while (lexer.LexToken(out _) is { } token)
            Items.Add(token);
        _lineLengths = lexer.LineLengths;
    }

    public readonly StringView GetSymbolContext(Token token) => Source.AsSpan(token.Start, token.Length);

    public readonly PositionConverter MakeConverter() => new(_lineLengths);

    public readonly Token? TryGetAt(Position pos) => TryGetAt(ProtocolToLocal(pos));

    public readonly StringView GetPrefix(Position position)
    {
        var offset = ProtocolToLocal(position);
        return TryGetAt(offset) is { } token
            ? Source.AsSpan(token.Start, offset - token.Start)
            : StringView.Empty;
    }

    public readonly void ProvideSemanticTokens(in SemanticTokensBuilder builder, IDefinitionsBag bag)
        => ProvideSemanticTokens(in builder, (0, Items.Count), bag);

    public readonly void ProvideSemanticTokens(in SemanticTokensBuilder builder, Range range, IDefinitionsBag bag)
    {
        FindRangeOuterBounds(ProtocolToLocal(range), out var left, out var right);
        ProvideSemanticTokens(in builder, (int.Max(left.index, 0), right.index), bag);
    }

    public void Rebuild(Range replacedRange, string replacement)
    {
        var itemsCount = Items.Count;
        var range = ProtocolToLocal(replacedRange);
        StringView sourceSpan = Source;
        Source = replacement.Length is 0
            ? string.Concat(sourceSpan[..range.start], sourceSpan[range.end..])
            : string.Concat(sourceSpan[..range.start], replacement, sourceSpan[range.end..]);

        FindRangeOuterBounds(range, out var firstTokenToReplace, out var firstTokenAfterRange);

        // Shift tokens after the range
        var shift = replacement.Length - (range.end - range.start);
        for (int i = firstTokenAfterRange.index; i < itemsCount; ++i)
            Items[i].Start += shift;
        firstTokenToReplace.offset = int.Max(firstTokenToReplace.offset, 0);

        var converter = MakeConverter();
        var startOffset = converter.LocalToProtocol(firstTokenToReplace.offset);
        Lexer lexer = new(Source, firstTokenToReplace.offset, startOffset.Character);

        List<Token> lexedTokens = [];
        // Short-circuit if replaced text chunk is at the end of source.
        if (firstTokenAfterRange.index == itemsCount)
        {
            while (lexer.LexToken(out _) is { } tok)
                lexedTokens.Add(tok);
            goto label_ReachedEOF;
        }

        Token? token;
        int tokenStart;
        do
        {
            token = lexer.LexToken(out tokenStart);
            // Token can be null in case last lexed token consumed last existing one before as its own part. Example:
            //   range end ↓    ↓ EOF
            // . . . .    123abc -> (num 123) A; (id 'abc') B;
            // . . . .   a123abc -> (id 'a123abc') C;
            // In this example C.start < B.start, but EOF was reached.
            if (token is null)
                goto label_ReachedEOF;

            if (tokenStart >= firstTokenAfterRange.offset)
                break;

            lexedTokens.Add(token);
        }
        while (true);

        // Until we run out of old tokens, compare to current one to check if we are repeating previous result
        for (var oldTokenIndex = firstTokenAfterRange.index; oldTokenIndex < itemsCount; ++oldTokenIndex)
        {
            var oldToken = Items[oldTokenIndex];
            var oldStart = oldToken.Start;
            // Lex tokens while any are left and last lexed token is 'behind' existing one.
            while (tokenStart < oldStart)
            {
                lexedTokens.Add(token);
                token = lexer.LexToken(out tokenStart);
                if (token is null)
                    goto label_ReachedEOF;
            }
            // If overshot, increase index of token compared with. We do not check the type equality, because it's
            // impossible for two tokens with different types to have the same offset.
            if (tokenStart > oldStart)
                continue;

            // Add current piece of parsed line to the list of lengths.
            lexer.CompleteLine();

            // Add remaining line length (first line was shifted in by Lexer already).
            // We use old lengths for calculations, so token.end must be corrected.
            var (endLine, character) = converter.LocalToProtocol(token.End - shift);
            var lexedLineLengths = lexer.LineLengths;
            lexedLineLengths[^1] += _lineLengths[endLine] - character;
            _lineLengths.ReplaceRange(in lexedLineLengths, (startOffset.Line, endLine + 1));
            Items.ReplaceRange(in lexedTokens, (int.Max(0, firstTokenToReplace.index), oldTokenIndex));
            return;
        }

        // If reached end of old token list, run lexer until EOF, then complement the end part.
        // Do not forget to add current token, which has first start after range end and thus was not added earlier
        for (; token is not null; token = lexer.LexToken(out _))
            lexedTokens.Add(token);

        label_ReachedEOF:
        _lineLengths.ReplaceEnd(in lexer.LineLengths, startOffset.Line);
        Items.ReplaceEnd(in lexedTokens, int.Max(0, firstTokenToReplace.index));
    }

    private readonly Token? TryGetAt(int offset)
    {
        for (int left = 0, right = Items.Count - 1; left <= right;)
        {
            var mid = left + (right - left) / 2;
            var current = Items[mid];
            if (offset <= current.Start)
            {
                right = mid - 1;
                continue;
            }
            if (offset <= current.End)
                return current;
            left = mid + 1;
        }
        return null;
    }

    private readonly int ProtocolToLocal(Position position)
    {
        var start = position.Character;
        for (int i = 0; i < position.Line; ++i)
            start += _lineLengths[i];
        return start;
    }

    private readonly RangeInfo ProtocolToLocal(Range range)
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
    /// Find range bounds in terms of not directly adjacent tokens.
    /// </summary>
    /// <param name="range">The range describing replaced text chunk.</param>
    /// <param name="firstBeforeRange">
    /// The tuple (index in token list, start offset), describing last token before the range, or (-1, -1) if such token
    /// is not present.
    /// </param>
    /// <param name="firstAfterRange">
    /// The tuple (index in token list, start offset), describing first token after the range, or (list.Count, -1)
    /// if such token is not present.
    /// </param>
    private readonly void FindRangeOuterBounds(RangeInfo range, out TokenInfo firstBeforeRange,
        out TokenInfo firstAfterRange)
    {
        // To replace tokens, we first find the last token before and not touching replaced range.
        // We use its end position as a start for Lexer run.
        var maxTokenPos = Items.Count - 1;
        firstBeforeRange = (-1, 0);
        for (int left = 0, right = maxTokenPos; left <= right;)
        {
            var mid = left + (right - left) / 2;
            var current = Items[mid];
            var start = current.Start;
            if (current.End < range.start)
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
            for (var prev = Items[right]; prev.End < range.start;)
            {
                firstBeforeRange = (right, prev.Start);
                break;
            }
        }

        firstAfterRange = (Items.Count, -1);
        for (int left = firstBeforeRange.index + 1, right = maxTokenPos; left <= right;)
        {
            var mid = left + (right - left) / 2;
            var current = Items[mid];
            if (current.End < range.start)
            {
                // Similar situation: now we only record rightmost tokens to the right of the range.
                left = mid + 1;
                continue;
            }
            var start = current.Start;
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
            var nextStart = Items[left].Start;
            if (nextStart > range.end)
            {
                firstAfterRange = (left, nextStart);
            }
        }
    }

    // TODO incremental
    private readonly void ProvideSemanticTokens(in SemanticTokensBuilder builder, RangeInfo bounds, IDefinitionsBag bag)
    {
        var converter = MakeConverter();
        for (int i = bounds.start; i < bounds.end; ++i)
        {
            var token = Items[i];
            if (token is CommentToken comment)
            {
                foreach (var (offset, length) in converter.LocalToProtocol(comment))
                    builder.Push(offset, length, comment.SemanticType);
                continue;
            }
            var type = token switch
            {
                IdentifierToken id when bag.Has(id, out var definitions) => definitions[0].SemanticType,
                { SemanticType: { } semanticType } => semanticType,
                _ => null
            };
            if (type is not null)
                builder.Push(converter.LocalToProtocol(token.Start), token.Length, type);
        }
    }

    public string Source { get; private set; }
    public readonly List<Token> Items;
    private readonly List<int> _lineLengths;
}