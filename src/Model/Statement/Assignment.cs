using NMLServer.Lexing;
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
    public override int end => _semicolon?.end ?? (_rightHandSide?.end ?? (_equalsSign?.end ?? _leftHandSide.end));

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
        _rightHandSide = ExpressionAST.TryParse(ref state);
        _semicolon = state.ExpectSemicolon();
    }

    // TODO
    // public override void ProvideDiagnostics(dynamic context)
    // {
    //     if (_equalsSign is null)
    //     {
    //         context.AddError("Unexpected expression at top level of the script", _leftHandSide);
    //         return;
    //     }
    //     switch (_leftHandSide)
    //     {
    //         // TODO: add support for previously defined param names
    //         case Identifier { kind: var kind } when kind.HasFlag(SymbolKind.UserDefined):
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
    //                 context.AddError("Expected square bracket for var/param invocation", opening.start, 1);
    //             }
    //             if (closing is null)
    //             {
    //                 context.AddError("Missing closing bracket", arguments.end);
    //             }
    //             else if (closing.Bracket is not ']')
    //             {
    //                 context.AddError("Expected square bracket for var/param invocation", closing.start, 1);
    //             }
    //             break;
    //         }
    //         default:
    //             context.AddError("Invalid parameter assignment target", _leftHandSide);
    //             break;
    //     }
    //     if (_rightHandSide is null)
    //     {
    //         context.AddError("Missing assigned value", _equalsSign.start + 1);
    //         if (_semicolon is null)
    //         {
    //             context.AddError("Missing semicolon", _equalsSign.start + 1);
    //         }
    //         return;
    //     }
    //     // _rightHandSide.ProvideDiagnostics(context);
    //     if (_semicolon is null)
    //     {
    //         context.AddError("Missing semicolon", _rightHandSide);
    //     }
    // }
}