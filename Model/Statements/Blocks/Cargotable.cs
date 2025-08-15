using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal sealed class Cargotable(ref ParsingState state, KeywordToken keyword)
    : BaseBlockStatement<ValueWithComma<BaseValueToken>>(ref state, keyword);
