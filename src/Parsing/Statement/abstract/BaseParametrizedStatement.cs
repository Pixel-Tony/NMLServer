using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal abstract class BaseParametrizedStatement : BaseStatement
{
    protected readonly KeywordToken Keyword;
    protected readonly ExpressionAST? Parameters;

    protected BaseParametrizedStatement(ParsingState state, KeywordToken keyword)
    {
        Keyword = keyword;
        state.Increment();
        Parameters = ExpressionAST.TryParse(state);
    }
}