using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal sealed class FunctionLikeStatement : BaseStatement
{
    private readonly KeywordToken _keyword;
    private readonly ExpressionAST? _parameters;
    private readonly SemicolonToken? _semicolon;

    public FunctionLikeStatement(ParsingState state, KeywordToken keyword)
    {
        _keyword = keyword;
        state.IncrementSkippingComments();
        _parameters = ExpressionAST.TryParse(state);
        _semicolon = state.ExpectSemicolon();
    }
}