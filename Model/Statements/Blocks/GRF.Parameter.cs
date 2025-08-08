using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal partial class GRF
{
    private sealed partial class Parameter(ref ParsingState state, KeywordToken keyword)
        : BaseBlockStatement<Parameter.Setting>(ref state, keyword), IFoldingRangeProvider
    {
        public override void ProvideFoldingRanges(in List<FoldingRange> ranges, ref readonly PositionConverter converter)
            => IFoldingRangeProvider.RangeWithInnerRanges(OpeningBracket, ClosingBracket, Contents, in ranges, in converter);
    }
}