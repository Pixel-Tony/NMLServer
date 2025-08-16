using NMLServer.Extensions;
using NMLServer.Model.Tokens;
using NMLServer.Model.Expressions;
using DotNetGraph.Core;

namespace NMLServer.Model.Statements;

internal readonly struct PropertySetting : IBlockContents<PropertySetting>
{
    private readonly BaseMulticharToken? _key;
    private readonly ColonToken? _colon;
    private readonly BaseExpression? _value;
    private readonly SemicolonToken? _semicolon;

    public int End => _semicolon?.End ?? _value?.End ?? _colon?.End ?? _key!.End;

    public PropertySetting(BaseMulticharToken? key, ColonToken? colon, BaseExpression? value, SemicolonToken? semicolon)
    {
        _key = key;
        _colon = colon;
        _value = value;
        _semicolon = semicolon;
    }

    public PropertySetting(ref ParsingState state, ColonToken colon)
    {
        _colon = colon;
        state.Increment();
        _value = BaseExpression.TryParse(ref state);
        _semicolon = state.ExpectSemicolon();
    }

    public PropertySetting(ref ParsingState state, BaseMulticharToken key)
    {
        _key = key;
        while (state.NextToken is { } token)
        {
            switch (token)
            {
                case ColonToken colonToken:
                    _colon = colonToken;
                    state.Increment();
                    _value = BaseExpression.TryParse(ref state);
                    _semicolon = state.ExpectSemicolon();
                    return;

                case SemicolonToken semicolonToken:
                    _semicolon = semicolonToken;
                    state.Increment();
                    return;

                case BracketToken { Bracket: '{' or '}' }:
                case KeywordToken { IsDefiningStatement: true }:
                    return;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
    }

    public static List<PropertySetting>? ParseSomeInBlock(ref ParsingState state, ref BracketToken? closingBracket)
    {
        List<PropertySetting> attributes = [];
        while (state.CurrentToken is { } token)
        {
            switch (token)
            {
                case ColonToken colonToken:
                    attributes.Add(new PropertySetting(ref state, colonToken));
                    break;

                case BaseValueToken identifierToken:
                    attributes.Add(new PropertySetting(ref state, identifierToken));
                    break;

                case BracketToken { Bracket: '}' } expectedClosingBracket:
                    closingBracket = expectedClosingBracket;
                    state.Increment();
                    goto label_End;

                case KeywordToken { IsDefiningStatement: true }:
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

    public DotNode Visualize(DotGraph graph, DotNode parent, StringView ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "Attr").WithStmtFeatures();
        _key.MaybeVisualize(graph, n, ctx);
        _colon.MaybeVisualize(graph, n, ctx);
        _value.MaybeVisualize(graph, n, ctx);
        _semicolon.MaybeVisualize(graph, n, ctx);
        return n;
    }
}