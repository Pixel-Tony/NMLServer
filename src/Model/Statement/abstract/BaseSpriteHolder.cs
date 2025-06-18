using NMLServer.Model.Lexis;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal abstract class BaseSpriteHolder(ref ParsingState state, KeywordToken keyword, BlockStatement.ParamInfo info)
    : BlockStatement<ExpressionAST>(ref state, keyword, info);