using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal class FunctionLikeStatement : BaseStatement
{
    private readonly KeywordToken _keyword;
    protected readonly ExpressionAST? Parameters;
    private SemicolonToken? _semicolon;

    public FunctionLikeStatement(ParsingState state, KeywordToken keyword)
    {
        _keyword = keyword;
        state.Increment();
        Parameters = ExpressionAST.TryParse(state);
        _semicolon = state.ExpectSemicolonAfterExpression();
    }
}