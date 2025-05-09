using NMLServer.Extensions;
using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal sealed class Assignment : StatementAST, ISymbolSource
{
    private readonly ExpressionAST _leftHandSide;
    private readonly AssignmentToken? _equalsSign;
    private readonly ExpressionAST? _rightHandSide;
    private readonly SemicolonToken? _semicolon;

    public IdentifierToken? symbol { get; }

    public override int start => _leftHandSide.start;
    public override int end => _semicolon?.end ?? _rightHandSide?.end ?? _equalsSign?.end ?? _leftHandSide.end;

    public Assignment(ref ParsingState state)
    {
        _leftHandSide = ExpressionAST.TryParse(ref state)!;
        if (_leftHandSide is Identifier { kind: SymbolKind.Undefined } identifier)
        {
            identifier.kind = SymbolKind.Parameter | SymbolKind.Writeable | SymbolKind.UserDefined;
            symbol = identifier.token;
        }
        for (var token = state.currentToken; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' or '}' }:
                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.CallDefining }:
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
        _rightHandSide = ExpressionAST.TryParse(ref state);
        _semicolon = state.ExpectSemicolon();
    }

    // TODO
    // public void ProvideDiagnostics(ref readonly DiagnosticContext context)
    // {
    //     if (_equalsSign is null)
    //     {
    //         context.AddError(Errors.UnexpectedTopLevelExpr, _leftHandSide);
    //         return;
    //     }
    //     switch (_leftHandSide)
    //     {
    //         // TODO: add support for previously defined param names
    //         case Identifier { kind: var kind } when kind.IsFlaggedBy(SymbolKind.UserDefined):
    //             break;
    //
    //         case FunctionCall
    //         {
    //             Function: KeywordToken { Type: KeywordType.Param },
    //             Arguments: { OpeningBracket: var opening, ClosingBracket: var closing } arguments
    //         }:
    //         {
    //             if (opening!.Bracket is not '[')
    //             {
    //                 context.AddError(Errors.ExpectedSquareBracket, opening.start, 1);
    //             }
    //             if (closing is null)
    //             {
    //                 context.AddError(Errors.MissingClosingBracket, arguments.end);
    //             }
    //             else if (closing.Bracket is not ']')
    //             {
    //                 int closingStart = closing.start;
    //                 context.AddError(Errors.ExpectedSquareBracket, closingStart, closingStart + 1);
    //             }
    //             break;
    //         }
    //
    //         default:
    //             context.AddError(Errors.InvalidTarget, _leftHandSide);
    //             break;
    //     }
    //     if (_rightHandSide is null)
    //     {
    //         context.AddError(Errors.MissingAssignedValue, _equalsSign.start + 1);
    //         if (_semicolon is null)
    //         {
    //             context.AddError(Errors.MissingSemicolon, _equalsSign.start + 1);
    //         }
    //         return;
    //     }
    //     // TODO _rightHandSide.ProvideDiagnostics(ref context);
    //     if (_semicolon is null)
    //     {
    //         context.AddError(Errors.MissingSemicolon, _rightHandSide);
    //     }
    // }
}