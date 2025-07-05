using NMLServer.Model.Lexis;

namespace NMLServer.Model;

internal struct ParsingState(IReadOnlyList<Token> tokens, int offset = 0)
{
    private readonly int _max = tokens.Count - 1;
    public readonly List<Token> UnexpectedTokens = [];
    private int _offset = offset;

    public readonly void AddUnexpected(Token token)
    {
        if (token is not CommentToken)
            UnexpectedTokens.Add(token);
    }

    public void Increment() => ++_offset;

    public readonly Token? CurrentToken => _offset <= _max ? tokens[_offset] : null;

    public Token? NextToken => ++_offset <= _max ? tokens[_offset] : null;

    public SemicolonToken? ExpectSemicolon()
    {
        for (var token = CurrentToken; token is not null; token = NextToken)
        {
            switch (token)
            {
                case SemicolonToken semicolon:
                    IncrementSkippingComments();
                    return semicolon;
                case BracketToken { Bracket: '}' }:
                case KeywordToken { Kind: KeywordKind.BlockDefining }:
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
        for (var token = CurrentToken; token is not null; token = NextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' } closingBracket:
                    IncrementSkippingComments();
                    return closingBracket;
                case KeywordToken { Kind: KeywordKind.BlockDefining }:
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
        while (CurrentToken is CommentToken);
    }
}