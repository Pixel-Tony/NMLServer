using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing;

internal class ParsingState
{
    private readonly Token[] _tokens;
    private readonly int _max;
    private int _pointer;
    private readonly List<Token> _unexpectedTokens = new();

    public IEnumerable<Token> unexpectedTokens => _unexpectedTokens;

    public void AddUnexpected(Token? token)
    {
        if (token is not null)
        {
            _unexpectedTokens.Add(token);
        }
    }

    public ParsingState(IEnumerable<Token> tokens)
    {
        _tokens = tokens.ToArray();
        _pointer = 0;
        _max = _tokens.Length - 1;
    }

    public void Increment() => ++_pointer;

    public Token? currentToken => _pointer <= _max
        ? _tokens[_pointer]
        : null;

    public Token? nextToken => ++_pointer <= _max
        ? _tokens[_pointer]
        : null;

    public SemicolonToken? ExpectSemicolonAfterExpression()
    {
        for (var token = currentToken; token is not null; token = nextToken)
        {
            switch (token)
            {
                case SemicolonToken semicolon:
                    Increment();
                    return semicolon;

                case BracketToken { Bracket: '}' }:
                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                    return null;

                default:
                    AddUnexpected(token);
                    break;
            }
        }
        return null;
    }

    public (bool isClosingInstead, BracketToken? token) ExpectOpeningCurlyBracket()
    {
        for (var token = currentToken; token is not null; token = nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' } openingBracket:
                    Increment();
                    return (false, openingBracket);

                case BracketToken { Bracket: '}' } closingBracket:
                    Increment();
                    return (true, closingBracket);

                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                    return (false, null);

                default:
                    AddUnexpected(token);
                    break;
            }
        }
        return (false, null);
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

                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                case BracketToken { Bracket: '{' }:
                    return null;

                default:
                    AddUnexpected(token);
                    break;
            }
        }
        return null;
    }
}