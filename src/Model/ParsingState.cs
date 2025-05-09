using NMLServer.Model.Lexis;

namespace NMLServer.Model;

internal struct ParsingState(IReadOnlyList<Token> tokens, in List<Token> unexpectedTokens, int offset = 0)
{
    private readonly int _max = tokens.Count - 1;
    private readonly List<Token> _unexpectedTokens = unexpectedTokens;
    private int _offset = offset;

    public void AddUnexpected(Token? token)
    {
        if (token is not (null or CommentToken))
            _unexpectedTokens.Add(token);
    }

    public void Increment() => ++_offset;

    public Token? currentToken => _offset <= _max ? tokens[_offset] : null;

    public Token? nextToken => ++_offset <= _max ? tokens[_offset] : null;

    public SemicolonToken? ExpectSemicolon()
    {
        for (var token = currentToken; token is not null; token = nextToken)
        {
            switch (token)
            {
                case SemicolonToken semicolon:
                    Increment();
                    return semicolon;
                case BracketToken { Bracket: '}' }:
                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.CallDefining }:
                    return null;
                default:
                    AddUnexpected(token);
                    break;
            }
        }
        return null;
    }

    public BracketToken? ExpectClosingCurlyBracket()
    {
        for (var token = currentToken; token is not null; token = nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' } closingBracket:
                    Increment();
                    return closingBracket;
                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.CallDefining }:
                case BracketToken { Bracket: '{' }:
                    return null;
                default:
                    AddUnexpected(token);
                    break;
            }
        }
        return null;
    }

    public void IncrementSkippingComments()
    {
        do
            Increment();
        while (currentToken is CommentToken);
    }
}