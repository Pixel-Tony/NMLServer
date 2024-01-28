using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal readonly struct ProduceCargoList
{
    private readonly BracketToken? _openingBracket;
    private readonly NMLAttribute[]? _content;
    private readonly BracketToken? _closingBracket;

    public ProduceCargoList(BracketToken closingBracket)
    {
        _closingBracket = closingBracket;
    }

    public ProduceCargoList(ParsingState state, BracketToken openingBracket)
    {
        List<NMLAttribute> content = new();
        _openingBracket = openingBracket;
        for (var token = state.nextToken; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: ']' } closingBracket:
                    _closingBracket = closingBracket;
                    state.Increment();

                    _content = content.ToMaybeArray();
                    return;

                case ColonToken colonToken:
                    content.Add(new NMLAttribute(state, colonToken));
                    break;

                case IdentifierToken identifierToken:
                    content.Add(new NMLAttribute(state, identifierToken));
                    break;

                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
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