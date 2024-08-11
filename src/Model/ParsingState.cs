using System.Runtime.CompilerServices;
using NMLServer.Lexing;

namespace NMLServer.Model;

internal ref struct ParsingState(IReadOnlyList<Token> tokens, in List<Token> unexpectedTokens, int offset = 0)
{
    private readonly int _max = tokens.Count - 1;
    private readonly IReadOnlyList<Token> _tokens = tokens;
    private readonly List<Token> _unexpectedTokens = unexpectedTokens;

    public void AddUnexpected(Token? token)
    {
        if (token is not (null or CommentToken))
        {
            _unexpectedTokens.Add(token);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Increment() => ++offset;

    // TODO: replace bounds check with additional 'null' value in the end of a list
    public Token? currentToken => offset <= _max
        ? _tokens[offset]
        : null;

    public Token? nextToken => ++offset <= _max
        ? _tokens[offset]
        : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IncrementSkippingComments()
    {
        do
        {
            Increment();
        }
        while (currentToken is CommentToken);
    }
}