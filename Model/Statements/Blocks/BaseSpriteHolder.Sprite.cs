using NMLServer.Extensions;
using NMLServer.Model.Expressions;
using NMLServer.Model.Tokens;
using DotNetGraph.Core;
using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;

namespace NMLServer.Model.Statements.Blocks;

partial class BaseSpriteHolder
{
    public readonly struct Sprite(
        IdentifierToken? label,
        ColonToken? labelColon,
        RecolourSprite? recolourSprite = null,
        BaseExpression? realOrTemplate = null) : IBlockContents<Sprite>, IFoldingRangeProvider
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
                        var realOrTemplate = BaseExpression.TryParse(ref state)!;
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
                        var realSprite = BaseExpression.TryParse(ref state)!;
                        result.Add(new Sprite(label, labelColon, null, realSprite));
                        break;

                    case KeywordToken { Keyword: Grammar.Keyword.RecolourSprite } kw:
                        var recolour = new RecolourSprite(ref state, kw);
                        result.Add(new Sprite(label, labelColon, recolour));
                        break;

                    case KeywordToken { IsDefiningStatement: true }:
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

        public DotNode Visualize(DotGraph graph, DotNode parent, StringView ctx)
            => VizExtensions.MakeNode(graph, parent, "Sprite")
                .WithStmtFeatures();

        void IFoldingRangeProvider.ProvideFoldingRanges(List<FoldingRange> ranges, ref PositionConverter converter)
            => recolourSprite?.ProvideFoldingRanges(ranges, ref converter);
    }
}