using NMLServer.Model.Tokens;
using NMLServer.Model.Expressions;
using NMLServer.Model.Grammar;

namespace NMLServer.Model.Statements.Procedures;

internal sealed partial class Produce : BaseStatement
{
    private readonly KeywordToken _keyword;
    private readonly BracketToken? _openingBracket;
    private readonly IdentifierToken? _id;
    private readonly BinaryOpToken? _firstComma;
    private readonly CargoList _consumptions;
    private readonly BinaryOpToken? _secondComma;
    private readonly CargoList _productions;
    private readonly BinaryOpToken? _thirdComma;
    private readonly BaseExpression? _runAgain;
    private readonly BracketToken? _closingBracket;

    public override int Start => _keyword.Start;

    public override int End => _closingBracket?.End ?? _runAgain?.End ?? _thirdComma?.End ?? _productions.End
        ?? _secondComma?.End ?? _consumptions.End ?? _firstComma?.End ?? _id?.End ?? _openingBracket?.End ?? _keyword.End;

    private enum InnerState
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

    public Produce(ref ParsingState state, KeywordToken keyword)
    {
        _keyword = keyword;
        state.Increment();
        InnerState innerState = InnerState.OpeningBracket;
        while (state.CurrentToken is { } token)
        {
            switch (token)
            {
                case KeywordToken { IsDefiningStatement: true }:
                    return;

                case BracketToken { Bracket: '(' } openingBracket when innerState is InnerState.OpeningBracket:
                    _openingBracket = openingBracket;
                    state.Increment();
                    innerState = InnerState.Id;
                    break;

                case BracketToken { Bracket: ')' } closingBracket:
                    _closingBracket = closingBracket;
                    state.Increment();
                    return;

                case BracketToken { Bracket: '[' } openingBracket:
                    if (innerState is InnerState.Consumptions)
                    {
                        _consumptions = new CargoList(ref state, openingBracket);
                        innerState = InnerState.SecondComma;
                        break;
                    }
                    if (innerState is InnerState.Productions)
                    {
                        _productions = new CargoList(ref state, openingBracket);
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
                        state.Increment();
                        innerState = InnerState.Consumptions;
                        break;
                    }
                    if (innerState is InnerState.SecondComma)
                    {
                        _secondComma = comma;
                        state.Increment();
                        innerState = InnerState.Productions;
                        break;
                    }
                    if (innerState is InnerState.ThirdComma)
                    {
                        _thirdComma = comma;
                        state.Increment();
                        innerState = InnerState.RunAgain;
                        break;
                    }
                    goto default;

                case IdentifierToken identifierToken when innerState is InnerState.Id:
                    _id = identifierToken;
                    state.Increment();
                    innerState = InnerState.FirstComma;
                    break;

                case BracketToken { Bracket: '(' }
                    or KeywordToken { IsExpressionUsable: true }
                    or UnaryOpToken
                    or BinaryOpToken
                    or TernaryOpToken
                    or BaseValueToken
                    when innerState is InnerState.RunAgain:

                    _runAgain = BaseExpression.TryParse(ref state);
                    /* Expression parser will consume final ')' paren as part of expression, we have to undo this */
                    if (_runAgain is ParentedExpression { ClosingBracket.Bracket: ']' } parented)
                    {
                        _closingBracket = parented.ClosingBracket;
                        _runAgain = parented.Expression;
                        state.Increment();
                        return;
                    }
                    innerState = InnerState.ClosingBracket;
                    break;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
    }
}