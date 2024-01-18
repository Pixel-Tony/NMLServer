using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class FunctionLikeStatement : BaseParametrizedStatement
{
    private SemicolonToken? _semicolon;

    public FunctionLikeStatement(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        _semicolon = state.ExpectSemicolonAfterExpression();
    }
}