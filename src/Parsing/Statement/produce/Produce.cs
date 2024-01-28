using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal sealed class Produce : BaseStatement
{
    private readonly KeywordToken _keyword;
    private readonly BracketToken? _openingBracket;
    private readonly IdentifierToken? _id;
    private readonly BinaryOpToken? _firstComma;
    private readonly BinaryOpToken? _secondComma;
    private readonly BinaryOpToken? _thirdComma;
    private readonly ExpressionAST? _runAgain;
    private readonly BracketToken? _closingBracket;
    private readonly ProduceCargoList _consumptions;
    private readonly ProduceCargoList _productions;

    private enum ParseFSM : byte
    {
        OpeningBracket = 0,
        Id = 1,
        FirstComma = 2,
        Consumptions = 3,
        SecondComma = 4,
        Productions = 5,
        ThirdComma = 6,
        RunAgain = 7,
        ClosingBracket = 8
    }

    public Produce(ParsingState state, KeywordToken keyword)
    {
        _keyword = keyword;
        var token = state.currentToken;
        ParseFSM localState = ParseFSM.OpeningBracket;
        for (; token is not null && _openingBracket is null; token = state.nextToken)
        {
            switch (token)
            {
                case KeywordToken { Type: not KeywordType.Return, IsExpressionUsable: false }:
                    return;

                case BracketToken { Bracket: '(' } openingBracket when localState is ParseFSM.OpeningBracket:
                    _openingBracket = openingBracket;
                    localState = ParseFSM.Id;
                    break;

                case BracketToken { Bracket: ')' } closingBracket:
                    _closingBracket = closingBracket;
                    state.Increment();
                    return;

                case BracketToken { Bracket: '[' } openingBracket when localState is ParseFSM.Consumptions:
                    _consumptions = new ProduceCargoList(state, openingBracket);
                    localState = ParseFSM.SecondComma;
                    break;
                case BracketToken { Bracket: ']' } closingBracket when localState is ParseFSM.Consumptions:
                    _consumptions = new ProduceCargoList(closingBracket);
                    localState = ParseFSM.SecondComma;
                    break;

                case BracketToken { Bracket: '[' } openingBracket when localState is ParseFSM.Productions:
                    _productions = new ProduceCargoList(state, openingBracket);
                    localState = ParseFSM.ThirdComma;
                    break;
                case BracketToken { Bracket: ']' } closingBracket when localState is ParseFSM.Productions:
                    _productions = new ProduceCargoList(closingBracket);
                    localState = ParseFSM.ThirdComma;
                    break;

                case BinaryOpToken { Type: OperatorType.Comma } comma when localState is ParseFSM.FirstComma:
                    _firstComma = comma;
                    localState = ParseFSM.Consumptions;
                    break;

                case BinaryOpToken { Type: OperatorType.Comma } comma when localState is ParseFSM.SecondComma:
                    _secondComma = comma;
                    localState = ParseFSM.Productions;
                    break;

                case BinaryOpToken { Type: OperatorType.Comma } comma when localState is ParseFSM.ThirdComma:
                    _thirdComma = comma;
                    localState = ParseFSM.RunAgain;
                    break;

                case IdentifierToken identifierToken when localState is ParseFSM.Id:
                    _id = identifierToken;
                    localState = ParseFSM.FirstComma;
                    break;

                case (
                    BracketToken { Bracket: '(' }
                    or KeywordToken { IsExpressionUsable: true }
                    or ValueToken
                    or UnaryOpToken
                    or BinaryOpToken
                    or TernaryOpToken
                    ) when localState is ParseFSM.RunAgain:
                    _runAgain = ExpressionAST.TryParse(state);
                    /* Expression parser will consume final ')' paren as part of expression, we have to undo this */
                    if (_runAgain is ParentedExpression
                        {
                            ClosingBracket: { Bracket: ')' } bracket,
                            Expression: var inner
                        })
                    {
                        _runAgain = inner;
                        _closingBracket = bracket;
                        state.Increment();
                        return;
                    }
                    localState = ParseFSM.ClosingBracket;
                    break;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
    }
}