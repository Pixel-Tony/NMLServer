using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal readonly record struct TownNamesPart
{
    private readonly BracketToken _openingBracket;
    private readonly BracketToken? _closingBracket;
    private readonly (ExpressionAST? call, BinaryOpToken? comma)[]? _texts;
    private readonly (KeywordToken townNamesKeyword, ExpressionAST? args, BinaryOpToken? comma)[]? _subParts;

    public TownNamesPart(ParsingState state, BracketToken openingBracket)
    {
        _openingBracket = openingBracket;
        List<(ExpressionAST? call, BinaryOpToken? comma)> texts = new();
        List<(KeywordToken townNamesKeyword, ExpressionAST? args, BinaryOpToken? comma)> subParts = new();
        for (var token = state.nextToken; token is not null;)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' } closingBracket:
                    _closingBracket = closingBracket;
                    state.Increment();
                    goto label_End;

                case IdentifierToken:
                    var call = ExpressionAST.TryParse(state, true);
                    token = state.currentToken;
                    if (token is BinaryOpToken { Type: OperatorType.Comma } commaInText)
                    {
                        texts.Add((call, commaInText));
                        break;
                    }
                    texts.Add((call, null));
                    continue;

                case KeywordToken { Type: KeywordType.TownNames } subPartKeyword:
                    state.IncrementSkippingComments();
                    var args = ExpressionAST.TryParse(state, true);

                    token = state.currentToken;
                    if (token is BinaryOpToken { Type: OperatorType.Comma } commaInSubPart)
                    {
                        subParts.Add((subPartKeyword, args, commaInSubPart));
                        break;
                    }
                    subParts.Add((subPartKeyword, args, null));
                    continue;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    goto label_End;

                default:
                    state.AddUnexpected(token);
                    break;
            }
            token = state.nextToken;
        }
        label_End:
        _texts = texts.ToArrayOrNull();
        _subParts = subParts.ToArrayOrNull();
    }
}