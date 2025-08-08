using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal sealed class SnowLine(ref ParsingState state, KeywordToken keyword)
    : BaseBlockStatement<PropertySetting>(ref state, keyword);