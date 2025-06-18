using DotNetGraph.Core;
using NMLServer.Extensions;
using NMLServer.Model.Lexis;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal readonly struct ItemGraphicsAttribute : IAllowsParseInsideBlock<ItemGraphicsAttribute>
{
    private readonly IdentifierToken? _identifier;
    private readonly ColonToken? _colon;
    private readonly KeywordToken? _returnKeyword;
    private readonly ExpressionAST? _value;
    private readonly SemicolonToken? _semicolon;

    public int end => _semicolon?.end ?? (_value?.end ?? (_returnKeyword?.end ?? (_colon?.end ?? _identifier!.end)));

    public static List<ItemGraphicsAttribute>? ParseSomeInBlock(ref ParsingState state,
        ref BracketToken? expectedClosingBracket)
    {
        List<ItemGraphicsAttribute> attributes = [];

        for (var token = state.currentToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' } closingBracket:
                    expectedClosingBracket = closingBracket;
                    state.Increment();
                    goto label_End;

                case ColonToken colonToken:
                    attributes.Add(new ItemGraphicsAttribute(ref state, colonToken));
                    break;

                case IdentifierToken identifierToken:
                    attributes.Add(new ItemGraphicsAttribute(ref state, identifierToken));
                    break;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.CallDefining }:
                    goto label_End;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        label_End:
        return attributes.ToMaybeList();
    }

    private ItemGraphicsAttribute(ref ParsingState state, IdentifierToken identifier)
    {
        _identifier = identifier;
        for (var token = state.nextToken; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' }:
                    return;

                case ColonToken colonToken:
                    _colon = colonToken;
                    state.IncrementSkippingComments();
                    token = state.currentToken;
                    if (token is KeywordToken { Type: KeywordType.Return } returnKeyword)
                    {
                        _returnKeyword = returnKeyword;
                        state.IncrementSkippingComments();
                    }
                    _value = ExpressionAST.TryParse(ref state);
                    _semicolon = state.ExpectSemicolon();
                    return;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.CallDefining }:
                    return;

                case SemicolonToken semicolonToken:
                    _semicolon = semicolonToken;
                    state.Increment();
                    return;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
    }

    private ItemGraphicsAttribute(ref ParsingState state, ColonToken colon)
    {
        _colon = colon;
        state.IncrementSkippingComments();
        _value = ExpressionAST.TryParse(ref state);
        _semicolon = state.ExpectSemicolon();
    }

    public DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "GraphicsBlock").WithStmtFeatures();
        _identifier.MaybeVisualize(graph, n, ctx);
        _colon.MaybeVisualize(graph, n, ctx);
        _returnKeyword.MaybeVisualize(graph, n, ctx);
        _value.MaybeVisualize(graph, n, ctx);
        _semicolon.MaybeVisualize(graph, n, ctx);
        return n;
    }
}