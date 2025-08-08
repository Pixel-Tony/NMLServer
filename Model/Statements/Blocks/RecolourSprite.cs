using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal sealed partial class RecolourSprite(ref ParsingState state, KeywordToken keyword)
    : BaseBlockStatement<RecolourSprite.Line>(ref state, keyword);