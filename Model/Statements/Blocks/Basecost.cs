using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal sealed class Basecost(ref ParsingState state, KeywordToken keyword)
    : BaseBlockStatement<PropertySetting>(ref state, keyword);