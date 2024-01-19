using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class Cargotable : BaseStatementWithBlock
{
    private readonly (IdentifierToken? item, BinaryOpToken? comma)[]? _content;

    public Cargotable(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        IdentifierToken? current = null;
        List<(IdentifierToken? item, BinaryOpToken? comma)> content = new();
        for (var token = state.currentToken; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case BinaryOpToken { Type: OperatorType.Comma } comma when current is not null:
                    content.Add((current, comma));
                    current = null;
                    break;

                case IdentifierToken identifierToken when current is null:
                    current = identifierToken;
                    break;

                case BracketToken { Bracket: '}' } closingBracket:
                    ClosingBracket = closingBracket;
                    state.Increment();
                    _content = content.ToMaybeArray();
                    return;

                case KeywordToken { Type: not KeywordType.Return, IsExpressionUsable: false }:
                    _content = content.ToMaybeArray();
                    return;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
        _content = content.ToMaybeArray();
    }
}