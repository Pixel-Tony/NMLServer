using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal readonly record struct ItemGraphicsAttribute : IAllowsParseInsideBlock<ItemGraphicsAttribute>
{
    private readonly IdentifierToken? _identifier;
    private readonly ColonToken? _colon;
    private readonly KeywordToken? _returnKeyword;
    private readonly ExpressionAST? _value;
    private readonly SemicolonToken? _semicolon;

    public int end => _semicolon?.end ?? (_value?.end ?? (_returnKeyword?.end ?? (_colon?.end ?? _identifier!.end)));

    public static List<ItemGraphicsAttribute>? ParseSomeInBlock(ParsingState state,
        ref BracketToken? expectedClosingBracket)
    {
        List<ItemGraphicsAttribute> attributes = new();

        for (var token = state.currentToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' } closingBracket:
                    expectedClosingBracket = closingBracket;
                    state.Increment();
                    return attributes.ToMaybeList();

                case ColonToken colonToken:
                    attributes.Add(new ItemGraphicsAttribute(state, colonToken));
                    break;

                case IdentifierToken identifierToken:
                    attributes.Add(new ItemGraphicsAttribute(state, identifierToken));
                    break;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    return attributes.ToMaybeList();

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        return attributes.ToMaybeList();
    }

    private ItemGraphicsAttribute(ParsingState state, IdentifierToken identifier)
    {
        _identifier = identifier;
        for (var token = state.nextToken; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' }:
                    return;

                case ColonToken colonToken:
                    _colon = colonToken;
                    state.IncrementSkippingComments();
                    token = state.currentToken;
                    if (token is KeywordToken { Type: KeywordType.Return } returnKeyword)
                    {
                        _returnKeyword = returnKeyword;
                        state.IncrementSkippingComments();
                    }
                    _value = ExpressionAST.TryParse(state);
                    _semicolon = state.ExpectSemicolon();
                    return;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    return;

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
        state.IncrementSkippingComments();
        _value = ExpressionAST.TryParse(state);
        _semicolon = state.ExpectSemicolon();
    }
}