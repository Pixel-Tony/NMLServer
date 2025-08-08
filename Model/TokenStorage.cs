using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using NMLServer.Extensions;
using NMLServer.Model.Tokens;

namespace NMLServer.Model;

using RangeInfo = (int start, int end);

internal struct TokenStorage
{
    public TokenStorage(string initialSource)
    {
        Source = initialSource;
        Lexer lexer = new(Source);
        while (lexer.LexToken() is { } token)
            Items.Add(token);
        _lineLengths = lexer.LineLengths;
    }

    public readonly StringView GetSymbolContext(BaseToken token) => Source.AsSpan(token.Start, token.Length);

    public readonly PositionConverter MakeConverter() => new(_lineLengths);

    public readonly BaseToken? TryGetAt(Position pos) => TryGetAt(ProtocolToLocal(pos));

    public readonly void ProvideSemanticTokens(in SemanticTokensBuilder builder, ref readonly DefinitionProcessor bag)
        => ProvideSemanticTokens(in builder, (0, Items.Count), in bag);

    public readonly void ProvideSemanticTokens(in SemanticTokensBuilder builder, Range range, ref readonly DefinitionProcessor bag)
    {
        var localRange = ProtocolToLocal(range);
        var start = Items.FindLastBefore(localRange.start);
        var end = Items.FindFirstAfter(localRange.end, start + 1);
        ProvideSemanticTokens(in builder, (int.Max(start, 0), end), in bag);
    }

    public RangeInfo Amend(Range? replacedRange, string replacement)
    {
        // Amend string contents
        var oldItemsCount = Items.Count;
        var (start, end) = replacedRange is { } rng ? ProtocolToLocal(rng) : (0, oldItemsCount);
        StringView sourceSpan = Source;
        Source = replacement.Length is 0
            ? string.Concat(sourceSpan[..start], sourceSpan[end..])
            : string.Concat(sourceSpan[..start], replacement, sourceSpan[end..]);

        // Calculate minimum token bounds to amend
        var firstDirtyToken = Items.FindLastBefore(start);
        var firstCleanToken = Items.FindFirstAfter(end, firstDirtyToken + 1);
        var startOffset = firstDirtyToken is -1 ? 0 : Items[firstDirtyToken].Start;

        // Shift tokens after the changed region
        var shift = replacement.Length - (end - start);
        {
            var items = Items;
            Parallel.For(firstCleanToken, oldItemsCount, i => items[i].Start += shift);
        }

        var converter = MakeConverter();
        var startPos = converter.LocalToProtocol(startOffset);
        Lexer lexer = new(Source, startOffset, startPos.Character);
        List<BaseToken> lexedTokens = [];
        RangeInfo resultRange = new() { start = int.Max(firstDirtyToken, 0) };
        if (lexer.LexToken() is not { } token)
            goto label_ReachedEOF;

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
            // Add remaining line length (first line was shifted in by lexer already).
            lexer.CompleteLine();

            // We use old lengths for calculations, so token.end must be corrected.
            var (endLine, character) = converter.LocalToProtocol(token.End - shift);
            var lexedLineLengths = lexer.LineLengths;
            lexedLineLengths[^1] += _lineLengths[endLine] - character;
            _lineLengths.ReplaceRange((startPos.Line, endLine + 1), in lexedLineLengths);
            resultRange.end = oldTokenIndex;
            goto label_End;
        }
        do
            lexedTokens.Add(token);
        while ((token = lexer.LexToken()) is not null);
    label_ReachedEOF:
        _lineLengths.ReplaceRange((startPos.Line, _lineLengths.Count), in lexer.LineLengths);
        resultRange.end = Items.Count;
    label_End:
        Items.ReplaceRange(resultRange, in lexedTokens);
        return resultRange;
    }

    private readonly BaseToken? TryGetAt(int offset)
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

    // TODO incremental
    private readonly void ProvideSemanticTokens(in SemanticTokensBuilder builder, RangeInfo bounds, ref readonly DefinitionProcessor bag)
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
                IdentifierToken id when bag.Has(id, Source, out var definitions) => definitions[0].SemanticType,
                { SemanticType: { } semanticType } => semanticType,
                _ => null
            };
            if (type is not null)
                builder.Push(converter.LocalToProtocol(token.Start), token.Length, type);
        }
    }

    public string Source { readonly get; private set; }
    public readonly List<BaseToken> Items = [];
    private readonly List<int> _lineLengths;
}