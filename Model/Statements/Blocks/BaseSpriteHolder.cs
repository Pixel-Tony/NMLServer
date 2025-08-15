using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal abstract partial class BaseSpriteHolder(ref ParsingState state, KeywordToken keyword)
    : BaseBlockStatement<BaseSpriteHolder.Sprite>(ref state, keyword)
{
    public override void ProvideFoldingRanges(List<FoldingRange> ranges, ref PositionConverter converter)
        => IFoldingRangeProvider.RangeWithInnerRanges(OpeningBracket, ClosingBracket, Contents, ranges, ref converter);
}