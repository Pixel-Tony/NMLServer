using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal abstract class BaseSpriteHolder(ref ParsingState state, KeywordToken keyword)
    : StatementWithBlockOf<ExpressionAST>(ref state, keyword);