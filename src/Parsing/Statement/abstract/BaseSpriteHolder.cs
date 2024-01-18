using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal abstract class BaseSpriteHolder : BaseParametrizedStatementWithBlock
{
    protected ExpressionAST[]? Content;

    protected BaseSpriteHolder(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        Content = ExpressionAST.TryParseSequence(state);
        ClosingBracket = state.ExpectClosingCurlyBracket();
    }
}