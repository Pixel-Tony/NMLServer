using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal class PropertyBlock(ref ParsingState state, KeywordToken keyword)
    : BaseBlockStatement<PropertySetting>(ref state, keyword);