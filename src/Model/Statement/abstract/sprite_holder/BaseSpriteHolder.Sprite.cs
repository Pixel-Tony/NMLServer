#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
using NMLServer.Extensions.DotNetGraph;
#endif
using NMLServer.Extensions;
using NMLServer.Model.Expression;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

partial class BaseSpriteHolder
{
    public readonly struct Sprite(
        IdentifierToken? label,
        ColonToken? labelColon,
        RecolourSprite? recolourSprite = null,
        ExpressionAST? realOrTemplate = null) : IBlockContents<Sprite>
    {
        public int End => realOrTemplate?.End ?? recolourSprite?.End ?? labelColon?.End ?? label!.End;

        public static List<Sprite>? ParseSomeInBlock(ref ParsingState state, ref BracketToken? closingBracket)
        {
            List<Sprite> result = [];
            IdentifierToken? label = null;
            ColonToken? labelColon = null;
            while (state.CurrentToken is { } token)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '}' } br:
                        closingBracket = br;
                        state.Increment();
                        goto label_End;

                    case IdentifierToken id when label is null:
                        var realOrTemplate = ExpressionAST.TryParse(ref state)!;
                        if (realOrTemplate is Identifier)
                        {
                            label = id;
                            continue;
                        }
                        result.Add(new Sprite(null, null, null, realOrTemplate));
                        continue;

                    case ColonToken colon when (label is not null) & (labelColon is null):
                        labelColon = colon;
                        state.Increment();
                        continue;

                    case BracketToken { Bracket: '[' }:
                    case IdentifierToken when (label is not null) & (labelColon is not null):
                        var realSprite = ExpressionAST.TryParse(ref state)!;
                        result.Add(new Sprite(label, labelColon, null, realSprite));
                        break;

                    case KeywordToken { Type: KeywordType.RecolourSprite } kw:
                        var recolour = new RecolourSprite(ref state, kw);
                        result.Add(new Sprite(label, labelColon, recolour));
                        break;

                    case KeywordToken { Kind: KeywordKind.BlockDefining }:
                        goto label_End;

                    default:
                        state.AddUnexpected(token);
                        state.Increment();
                        continue;
                }
                label = null;
                labelColon = null;
            }
        label_End:
            if (label is not null | labelColon is not null)
                result.Add(new Sprite(label, labelColon));
            return result.ToMaybeList();
        }

#if TREE_VISUALIZER_ENABLED
        public DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        {
            return VizExtensions.MakeNode(graph, parent, "Sprite").WithStmtFeatures();
        }
#endif

    }
}