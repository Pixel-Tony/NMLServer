using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal class Assignment : BaseStatement
{
    private readonly ExpressionAST? _leftHandSide;
    private readonly AssignmentToken? _equalsSign;
    private readonly ExpressionAST? _righHandSide;
    private SemicolonToken? _semicolon;

    public Assignment(ParsingState state)
    {
        _leftHandSide = ExpressionAST.TryParse(state);

        for (var token = state.currentToken;
             _equalsSign is null && token is not null;
             token = state.nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' or '}' }:
                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                    return;

                case AssignmentToken equalsSign:
                    _equalsSign = equalsSign;
                    break;

                case SemicolonToken semicolon:
                    _semicolon = semicolon;
                    state.Increment();
                    return;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
        _righHandSide = ExpressionAST.TryParse(state);
        _semicolon = state.ExpectSemicolon();
    }
}