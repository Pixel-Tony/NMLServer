using System.Runtime.CompilerServices;
using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing;

internal class ParsingState
{
    private readonly IReadOnlyList<Token> _tokens;
    private readonly int _max;
    private int _pointer;
    private readonly List<Token> _unexpectedTokens = new();

    public IReadOnlyList<Token> unexpectedTokens => _unexpectedTokens;

    public void AddUnexpected(Token? token)
    {
        if (token is not (null or CommentToken))
        {
            _unexpectedTokens.Add(token);
        }
    }

    public ParsingState(IReadOnlyList<Token> tokens)
    {
        _tokens = tokens;
        _pointer = 0;
        _max = _tokens.Count - 1;
    }

    public void Increment() => ++_pointer;

    public Token? currentToken => _pointer <= _max
        ? _tokens[_pointer]
        : null;

    public Token? nextToken => ++_pointer <= _max
        ? _tokens[_pointer]
        : null;

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
                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    return null;

                default:
                    AddUnexpected(token);
                    break;
            }
        }
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BracketToken? ExpectClosingCurlyBracket()
    {
        for (var token = currentToken; token is not null; token = nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' } closingBracket:
                    Increment();
                    return closingBracket;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
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
        Increment();
        while (currentToken is CommentToken)
        {
            Increment();
        }
    }
}