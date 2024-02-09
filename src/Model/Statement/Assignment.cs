using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal sealed class Assignment : BaseStatement
{
    private readonly ExpressionAST? _leftHandSide;
    private readonly AssignmentToken? _equalsSign;
    private readonly ExpressionAST? _righHandSide;
    private SemicolonToken? _semicolon;

    public Assignment(ParsingState state)
    {
        _leftHandSide = ExpressionAST.TryParse(state);
        for (var token = state.currentToken; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' or '}' }:
                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    return;

                case AssignmentToken equalsSign:
                    _equalsSign = equalsSign;
                    state.IncrementSkippingComments();
                    goto label_End;

                case SemicolonToken semicolon:
                    _semicolon = semicolon;
                    state.Increment();
                    return;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
        label_End:
        _righHandSide = ExpressionAST.TryParse(state);
        _semicolon = state.ExpectSemicolon();
    }
}