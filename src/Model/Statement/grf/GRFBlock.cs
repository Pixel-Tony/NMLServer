using NMLServer.Extensions;
using NMLServer.Model.Lexis;
using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Extensions.DotNetGraph;
#endif

namespace NMLServer.Model.Statement;

internal sealed partial class GRFBlock : BlockStatement
{
    private readonly List<NMLAttribute>? _attributes;
    private readonly List<Parameter>? _parameters;

    protected override int? MiddleEnd => IHasEnd.LastOf(_attributes, _parameters);

    public GRFBlock(ref ParsingState state, KeywordToken keyword) : base(ref state, keyword, ParamInfo.None)
    {
        if (ClosingBracket is not null)
            return;

        List<NMLAttribute> attributes = [];
        List<Parameter> parameters = [];

        while (state.CurrentToken is { } token)
        {
            switch (token)
            {
                case IdentifierToken identifierToken:
                    attributes.Add(new NMLAttribute(ref state, identifierToken));
                    break;

                case ColonToken colonToken:
                    attributes.Add(new NMLAttribute(ref state, colonToken));
                    break;

                case KeywordToken { Type: KeywordType.Param } paramToken:
                    parameters.Add(new Parameter(ref state, paramToken));
                    break;

                case KeywordToken { Kind: KeywordKind.BlockDefining }:
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

    public override void ProvideFoldingRanges(in Stack<IFoldingRangeProvider> children, in List<FoldingRange> ranges, ref TokenStorage.PositionConverter converter)
    {
        IFoldingRangeProvider.RangeFromBrackets(OpeningBracket, ClosingBracket, in ranges, ref converter, true);
        IFoldingRangeProvider.Include(_parameters, in children);
    }

#if TREE_VISUALIZER_ENABLED
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
#endif
}