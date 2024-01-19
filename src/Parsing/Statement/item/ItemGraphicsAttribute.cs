using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal readonly record struct ItemGraphicsAttribute
{
    private readonly IdentifierToken? _identifier;
    private readonly ColonToken? _colon;
    private readonly KeywordToken? _returnKeyword;
    private readonly ExpressionAST? _value;
    private readonly SemicolonToken? _semicolon;

    public static ItemGraphicsAttribute[]? TryParseManyInBlock(ParsingState state, ref BracketToken? expectedClosingBracket)
    {
        List<ItemGraphicsAttribute> attributes = new();
        for (var token = state.currentToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' } closingBracket:
                    expectedClosingBracket = closingBracket;
                    state.Increment();
                    return attributes.ToMaybeArray();

                case ColonToken colonToken:
                    attributes.Add(new ItemGraphicsAttribute(state, colonToken));
                    break;

                case IdentifierToken identifierToken:
                    attributes.Add(new ItemGraphicsAttribute(state, identifierToken));
                    break;

                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                    return attributes.ToMaybeArray();

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        return attributes.ToMaybeArray();
    }

    private ItemGraphicsAttribute(ParsingState state, IdentifierToken identifier)
    {
        _identifier = identifier;
        for (var token = state.nextToken; _colon is null && token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' }:
                    return;

                case ColonToken colonToken:
                    _colon = colonToken;
                    token = state.nextToken;
                    if (token is KeywordToken { Type: KeywordType.Return } returnKeyword)
                    {
                        _returnKeyword = returnKeyword;
                        state.Increment();
                    }
                    _value = ExpressionAST.TryParse(state);
                    _semicolon = state.ExpectSemicolonAfterExpression();
                    return;

                case KeywordToken:
                    break;

                case SemicolonToken semicolonToken:
                    _semicolon = semicolonToken;
                    state.Increment();
                    return;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
    }

    private ItemGraphicsAttribute(ParsingState state, ColonToken colon)
    {
        _colon = colon;
        state.Increment();
        _value = ExpressionAST.TryParse(state);
        _semicolon = state.ExpectSemicolonAfterExpression();
    }
}