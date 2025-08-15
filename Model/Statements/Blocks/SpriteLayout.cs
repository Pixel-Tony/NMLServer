using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal sealed partial class SpriteLayout(ref ParsingState state, KeywordToken keyword)
    : BaseBlockStatement<SpriteLayout.Entry>(ref state, keyword)
{
    public override void ProvideFoldingRanges(List<FoldingRange> ranges, ref PositionConverter converter)
        => IFoldingRangeProvider.RangeWithInnerRanges(OpeningBracket, ClosingBracket, Contents, ranges, ref converter);
}