using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using NMLServer.Extensions;
using NMLServer.Model.Grammar;
using NMLServer.Model.Tokens;

namespace NMLServer.Model;

using RangeInfo = (int start, int end);

internal struct TokenStorage
{
    private string _source;
    private readonly List<int> _lineLengths;

    public readonly StringView Source => _source;
    public readonly List<BaseToken> Items = [];

    public TokenStorage(string initialSource)
    {
        _source = initialSource;
        Lexer lexer = new(_source);
        while (lexer.LexToken() is { } token)
            Items.Add(token);
        _lineLengths = lexer.LineLengths;
    }

    public readonly PositionConverter MakeConverter() => new(_lineLengths);

    public readonly BaseToken? At(Position pos) => At(ProtocolToLocal(pos));

    // TODO remove
    public readonly void ProvideSemanticTokens(SemanticTokensBuilder builder, DefinitionBag bag)
        => ProvideSemanticTokens(builder, (0, Items.Count), bag);

    // TODO remove
    public readonly void ProvideSemanticTokens(SemanticTokensBuilder builder, Range range, DefinitionBag bag)
    {
        var localRange = ProtocolToLocal(range);
        var start = Items.FindLastBefore<IHasEnd, int>(localRange.start);
        var end = Items.FindFirstAfter<IHasStart, int>(localRange.end, start + 1);
        ProvideSemanticTokens(builder, (int.Max(start, 0), end), bag);
    }

    public (AmendResult result, RangeInfo newTokens) Amend(Range? replacedRange, string replacement)
    {
        // Amend string contents
        var oldItemsCount = Items.Count;
        var sourceSpan = Source;
        var (start, end) = replacedRange is { } rng ? ProtocolToLocal(rng) : (0, sourceSpan.Length);
        _source = replacement.Length is 0
            ? string.Concat(sourceSpan[..start], sourceSpan[end..])
            : string.Concat(sourceSpan[..start], replacement, sourceSpan[end..]);

        // Calculate minimum token bounds to amend
        var firstDirtyToken = Items.FindLastBefore<IHasEnd, int>(start);
        var firstCleanToken = Items.FindFirstAfter<IHasStart, int>(end, firstDirtyToken + 1);
        var startOffset = firstDirtyToken is -1 ? 0 : Items[firstDirtyToken].Start;

        // Shift tokens after the changed region
        var shift = replacement.Length - (end - start);
        {
            var items = Items;
            Parallel.For(firstCleanToken, oldItemsCount, i => items[i].Start += shift);
        }
        var converter = MakeConverter();
        var startPos = converter.LocalToProtocol(startOffset);
        Lexer lexer = new(Source, startOffset);
        List<int> lexedLineLengths = lexer.LineLengths;
        List<BaseToken> lexedTokens = [];
        RangeInfo resultRange = (int.Max(firstDirtyToken, 0), 0);
        if (lexer.LexToken() is not { } token)
            goto label_ReachedEOF;

        Position oldEndPos, newEndPos;
        // Until we run out of old tokens, compare old to current and check if we are repeating previous result.
        for (var oldTokenIndex = firstCleanToken; oldTokenIndex < oldItemsCount; ++oldTokenIndex)
        {
            var oldStart = Items[oldTokenIndex].Start;
            while (token.Start < oldStart)
            {
                lexedTokens.Add(token);
                token = lexer.LexToken();
                if (token is null)
                    goto label_ReachedEOF;
            }
            if (token.Start > oldStart)
                continue;
            // Manually add remaining line length.
            lexer.CompleteLine();

            // We use old lengths for calculations, so token.end must be corrected.
            var (endLine, character) = oldEndPos = converter.LocalToProtocol(token.End - shift);
            lexedLineLengths[0] += startPos.Character;
            lexedLineLengths[^1] += _lineLengths[endLine] - character;
            _lineLengths.ReplaceRange(lexedLineLengths, startPos.Line, endLine + 1);

            resultRange.end = oldTokenIndex;
            newEndPos = new Position(startPos.Line + lexedLineLengths.Count, lexedLineLengths[^1]);
            goto label_End;
        }
        do
            lexedTokens.Add(token);
        while ((token = lexer.LexToken()) is not null);
    label_ReachedEOF:
        oldEndPos = new Position(_lineLengths.Count, _lineLengths[^1]);
        lexedLineLengths[0] += startPos.Character;
        _lineLengths.ReplaceRange(lexedLineLengths, startPos.Line);
        resultRange.end = Items.Count;
        newEndPos = new(_lineLengths.Count, _lineLengths[^1]);
    label_End:
        Items.ReplaceRange(lexedTokens, resultRange.start, resultRange.end);
        return ((startPos, oldEndPos, newEndPos), resultRange);
    }

    private readonly BaseToken? At(int offset)
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
        for (int i = 0, mx = position.Line; i < mx; ++i)
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

    // TODO incremental
    private readonly void ProvideSemanticTokens(SemanticTokensBuilder builder, RangeInfo bounds, DefinitionBag bag)
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
                IdentifierToken id when bag.Has(id, Source, out var definitions) => NML.GetSemanticTokenType(definitions[0].kind),
                { SemanticType: { } semanticType } => semanticType,
                _ => null
            };
            if (type is not null)
                builder.Push(converter.LocalToProtocol(token.Start), token.Length, type);
        }
    }
}