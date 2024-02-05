using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal sealed partial class Produce : BaseStatement
{
    private readonly KeywordToken _keyword;
    private readonly BracketToken? _openingBracket;
    private readonly IdentifierToken? _id;
    private readonly BinaryOpToken? _firstComma;
    private readonly BinaryOpToken? _secondComma;
    private readonly BinaryOpToken? _thirdComma;
    private readonly ExpressionAST? _runAgain;
    private readonly BracketToken? _closingBracket;
    private readonly CargoList _consumptions;
    private readonly CargoList _productions;

    private enum InnerState : byte
    {
        OpeningBracket,
        Id,
        FirstComma,
        Consumptions,
        SecondComma,
        Productions,
        ThirdComma,
        RunAgain,
        ClosingBracket
    }

    public Produce(ParsingState state, KeywordToken keyword)
    {
        _keyword = keyword;
        var token = state.currentToken;
        InnerState innerState = InnerState.OpeningBracket;
        for (; token is not null && _openingBracket is null; token = state.nextToken)
        {
            switch (token)
            {
                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    return;

                case BracketToken { Bracket: '(' } openingBracket when innerState is InnerState.OpeningBracket:
                    _openingBracket = openingBracket;
                    innerState = InnerState.Id;
                    break;

                case BracketToken { Bracket: ')' } closingBracket:
                    _closingBracket = closingBracket;
                    state.Increment();
                    return;

                case BracketToken { Bracket: '[' } openingBracket:
                    if (innerState is InnerState.Consumptions)
                    {
                        _consumptions = new CargoList(state, openingBracket);
                        innerState = InnerState.SecondComma;
                        break;
                    }
                    if (innerState is InnerState.Productions)
                    {
                        _productions = new CargoList(state, openingBracket);
                        innerState = InnerState.ThirdComma;
                        break;
                    }
                    goto default;

                case BracketToken { Bracket: ']' } closingBracket:
                    if (innerState is InnerState.Consumptions)
                    {
                        _consumptions = new CargoList(closingBracket);
                        innerState = InnerState.SecondComma;
                        break;
                    }
                    if (innerState is InnerState.Productions)
                    {
                        _productions = new CargoList(closingBracket);
                        innerState = InnerState.ThirdComma;
                        break;
                    }
                    goto default;

                case BinaryOpToken { Type: OperatorType.Comma } comma:
                    if (innerState is InnerState.FirstComma)
                    {
                        _firstComma = comma;
                        innerState = InnerState.Consumptions;
                        break;
                    }
                    if (innerState is InnerState.SecondComma)
                    {
                        _secondComma = comma;
                        innerState = InnerState.Productions;
                        break;
                    }
                    if (innerState is InnerState.ThirdComma)
                    {
                        _thirdComma = comma;
                        innerState = InnerState.RunAgain;
                        break;
                    }
                    goto default;

                case IdentifierToken identifierToken when innerState is InnerState.Id:
                    _id = identifierToken;
                    innerState = InnerState.FirstComma;
                    break;

                case BracketToken { Bracket: '(' }
                    or KeywordToken { Kind: KeywordKind.ExpressionUsable }
                    or UnaryOpToken
                    or BinaryOpToken
                    or TernaryOpToken
                    or BaseValueToken
                    when innerState is InnerState.RunAgain:

                    _runAgain = ExpressionAST.TryParse(state);
                    /* Expression parser will consume final ')' paren as part of expression, we have to undo this */
                    if (_runAgain is ParentedExpression parented)
                    {
                        _runAgain = parented.Expression;
                        _closingBracket = parented.ClosingBracket;
                        state.Increment();
                        return;
                    }
                    innerState = InnerState.ClosingBracket;
                    break;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
    }
}