using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal class GraphicsBlock(ref ParsingState state, KeywordToken keyword) : BaseSwitch(ref state, keyword);