using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed partial class SpriteLayout(ref ParsingState state, KeywordToken keyword)
    : BlockStatement<SpriteLayout.Entry>(ref state, keyword, new ParamInfo(false, (1, 1), (0, SymbolKind.Variable)))
{
    public override void ProvideFoldingRanges(in Stack<IFoldingRangeProvider> children,
        in List<FoldingRange> ranges, ref TokenStorage.PositionConverter converter)
    {
        IFoldingRangeProvider.RangeFromBrackets(OpeningBracket, ClosingBracket, in ranges, ref converter, true);
        IFoldingRangeProvider.Include(Contents, in children);
    }
}