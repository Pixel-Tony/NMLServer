using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal partial class GRFBlock
{
    private sealed partial class Parameter(ref ParsingState state, KeywordToken keyword)
        : BlockStatement<Parameter.Block>(ref state, keyword, new ParamInfo(false, (0, 1)))
    {
        public override void ProvideFoldingRanges(in Stack<IFoldingRangeProvider> children, in List<FoldingRange> ranges, ref TokenStorage.PositionConverter converter)
        {
            IFoldingRangeProvider.RangeFromBrackets(OpeningBracket, ClosingBracket, in ranges, ref converter, true);
            IFoldingRangeProvider.Include(Contents, in children);
        }
    }
}