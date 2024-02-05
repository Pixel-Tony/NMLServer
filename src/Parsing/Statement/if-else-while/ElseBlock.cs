using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal class ElseBlock : BaseStatement
{
    private readonly KeywordToken _elseKeyword;
    private readonly KeywordToken? _ifKeyword;
    private readonly ExpressionAST? _condition;
    private readonly BracketToken? _openingBracket;
    private readonly NMLFile _contents;
    private readonly BracketToken? _closingBracket;

    public ElseBlock(ParsingState state, KeywordToken keyword)
    {
        _elseKeyword = keyword;
        var token = state.nextToken;
        for (; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case KeywordToken { Type: KeywordType.If } ifKeyword:
                    _ifKeyword = ifKeyword;
                    token = state.nextToken;
                    goto label_ParsingElseIfCondition;

                case BracketToken { Bracket: '{' } openingBracket:
                    _openingBracket = openingBracket;
                    state.Increment();
                    _contents = new NMLFile(state, ref _closingBracket);
                    return;

                case BracketToken { Bracket: '}' } closingBracket:
                    _closingBracket = closingBracket;
                    state.Increment();
                    return;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    return;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
        label_ParsingElseIfCondition:
        for (; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' } openingBracket:
                    _openingBracket = openingBracket;
                    state.Increment();
                    _contents = new NMLFile(state, ref _closingBracket);
                    return;

                case BracketToken { Bracket: '}' } closingBracket:
                    _closingBracket = closingBracket;
                    state.Increment();
                    return;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    return;

                case BracketToken:
                case UnaryOpToken:
                case BinaryOpToken:
                case TernaryOpToken:
                case BaseValueToken:
                    _condition = ExpressionAST.TryParse(state);
                    token = state.currentToken;
                    goto label_ParsingOpeningBracket;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
        label_ParsingOpeningBracket:
        for (; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' } openingBracket:
                    _openingBracket = openingBracket;
                    state.Increment();
                    _contents = new NMLFile(state, ref _closingBracket);
                    return;

                case BracketToken { Bracket: '}' } closingBracket:
                    _closingBracket = closingBracket;
                    state.Increment();
                    return;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    return;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
        _contents = new NMLFile(state, ref _closingBracket);
    }
}