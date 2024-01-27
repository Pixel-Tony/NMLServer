using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal abstract class BaseSpriteHolder : BaseStatementWithBlock
{
    protected ExpressionAST[]? Content;

    protected BaseSpriteHolder(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        var expression = ExpressionAST.TryParse(state);
        if (expression is not null)
        {
            List<ExpressionAST> result = new();
            while (expression is not null)
            {
                result.Add(expression);
                expression = ExpressionAST.TryParse(state);
            }
            Content = result.ToArray();
        }
        ClosingBracket = state.ExpectClosingCurlyBracket();
    }
}