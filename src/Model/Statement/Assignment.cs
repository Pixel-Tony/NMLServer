using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal sealed class Assignment : BaseStatement, IValidatable, ISymbolSource
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

    public void ProvideDiagnostics(DiagnosticsContext context)
    {
        if (_equalsSign is null)
        {
            context.AddError("Unexpected expression at top level of the script", _leftHandSide);
            return;
        }
        switch (_leftHandSide)
        {
            // TODO: add support for random param names, check if mentioned earlier and is param name
            case Identifier { kind: var kind } when kind.HasFlag(SymbolKind.UserDefined):
                break;

            case FunctionCall
            {
                Function: KeywordToken { Type: KeywordType.Param },
                Arguments: { OpeningBracket: var opening, ClosingBracket: var closing } arguments
            }:
            {
                if (opening!.Bracket is not '[')
                {
                    context.AddError("Expected square bracket for var/param invocation", opening.Start, 1);
                }
                if (closing is null)
                {
                    context.AddError("Missing closing bracket", arguments.end);
                }
                else if (closing.Bracket is not ']')
                {
                    context.AddError("Expected square bracket for var/param invocation", closing.Start, 1);
                }
                break;
            }
            default:
                context.AddError("Invalid parameter assignment target", _leftHandSide);
                break;
        }
        if (_righHandSide is null)
        {
            context.AddError("Missing assigned value", _equalsSign.Start + 1);
            if (_semicolon is null)
            {
                context.AddError("Missing semicolon", _equalsSign.Start + 1);
            }
            return;
        }
        _righHandSide.ProvideDiagnostics(context);
        if (_semicolon is null)
        {
            context.AddError("Missing semicolon", _righHandSide);
        }
    }
}