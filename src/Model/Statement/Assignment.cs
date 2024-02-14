using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal sealed class Assignment : BaseStatement, ISymbolSource
{
    private readonly ExpressionAST _leftHandSide;
    private readonly AssignmentToken? _equalsSign;
    private readonly ExpressionAST? _righHandSide;
    private readonly SemicolonToken? _semicolon;

    public IdentifierToken? symbol { get; }

    public Assignment(ParsingState state)
    {
        _leftHandSide = ExpressionAST.TryParse(state)!;
        if (_leftHandSide is Identifier { kind: SymbolKind.None } identifier)
        {
            identifier.kind = SymbolKind.Parameter | SymbolKind.Writeable | SymbolKind.UserDefined;
            symbol = identifier.token;
        }
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