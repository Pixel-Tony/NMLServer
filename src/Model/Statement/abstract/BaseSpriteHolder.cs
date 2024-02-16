using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal abstract class BaseSpriteHolder : BaseStatementWithBlockOf<ExpressionAST>
{
    protected BaseSpriteHolder(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}