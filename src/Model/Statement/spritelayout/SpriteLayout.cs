using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed partial class SpriteLayout(ref ParsingState state, KeywordToken keyword)
    : BlockStatement<SpriteLayout.Entry>(ref state, keyword, new ParamInfo(false, (1, 1), (0, SymbolKind.Variable)));