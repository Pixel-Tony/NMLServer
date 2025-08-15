using NMLServer.Extensions;
using NMLServer.Model.Tokens;
using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Model.Grammar;

namespace NMLServer.Model.Statements.Blocks;

internal sealed partial class GRF : BaseBlockStatement
{
    private readonly List<PropertySetting>? _attributes;
    private readonly List<Parameter>? _parameters;

    protected override int? MiddleEnd => (_attributes, _parameters).LastOf();

    public GRF(ref ParsingState state, KeywordToken keyword) : base(ref state, keyword)
    {
        if (ClosingBracket is not null)
            return;
        List<PropertySetting> attributes = [];
        List<Parameter> parameters = [];
        while (state.CurrentToken is { } token)
        {
            switch (token)
            {
                case IdentifierToken identifierToken:
                    attributes.Add(new PropertySetting(ref state, identifierToken));
                    break;

                case ColonToken colonToken:
                    attributes.Add(new PropertySetting(ref state, colonToken));
                    break;

                case KeywordToken { Keyword: Grammar.Keyword.Param } paramToken:
                    parameters.Add(new Parameter(ref state, paramToken));
                    break;

                case KeywordToken { IsDefiningStatement: true }:
                    goto label_End;

                case BracketToken { Bracket: '}' } closingBracket:
                    ClosingBracket = closingBracket;
                    state.Increment();
                    goto label_End;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
    label_End:
        _attributes = attributes.ToMaybeList();
        _parameters = parameters.ToMaybeList();
    }

    public override void ProvideFoldingRanges(List<FoldingRange> ranges, ref PositionConverter converter)
        => IFoldingRangeProvider.RangeWithInnerRanges(OpeningBracket, ClosingBracket, _parameters, ranges, ref converter);

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = base.Visualize(graph, parent, ctx).WithLabel("GRF");
        foreach (var p in _parameters ?? [])
            p.Visualize(graph, n, ctx);
        foreach (var a in _attributes ?? [])
            a.Visualize(graph, n, ctx);
        ClosingBracket.MaybeVisualize(graph, n, ctx);
        return n;
    }
}