using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal partial class GRF
{
    private sealed partial class Parameter(ref ParsingState state, KeywordToken keyword)
        : BaseBlockStatement<Parameter.Setting>(ref state, keyword), IFoldingRangeProvider
    {
        public override void ProvideFoldingRanges(List<FoldingRange> ranges, ref PositionConverter converter)
            => IFoldingRangeProvider.RangeWithInnerRanges(OpeningBracket, ClosingBracket, Contents, ranges, ref converter);
    }
}