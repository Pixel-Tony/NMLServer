using System.Runtime.CompilerServices;
using NMLServer.Lexing.Tokens;
using NMLServer.Parsing;

namespace NMLServer.Lexing;

internal class Lexer
{
    private static readonly HashSet<char> _opStarts = new(Grammar.Operators.Select(o => o[0]));

    private readonly string _input;
    private readonly int _maxPos;
    private int _pos;

    private char charPointedAt => _input[_pos];

    public Lexer(string inputString)
    {
        _input = inputString;
        _maxPos = inputString.Length - 1;
    }

    private readonly List<Token> _tokens = new();
    private readonly List<Token> _comments = new();
    
    public (Token[] tokens, Token[] comments) Tokenize()
    {
        while (_pos <= _maxPos)
        {
            char c = charPointedAt;
            if (char.IsWhiteSpace(c))
            {
                _pos++;
                continue;
            }
            switch (c)
            {
                case ';':
                    _tokens.Add(ConsumeInto<SemicolonToken>(c));
                    continue;
                case ':':
                    _tokens.Add(ConsumeInto<ColonToken>(c));
                    continue;
                case '?':
                    _tokens.Add(ConsumeInto<TernaryOpToken>(c));
                    continue;
            }

            if (IsValidIdStartCharacter(c))
            {
                _tokens.Add(ParseIdentifier(c));
                continue;
            }

            if (char.IsDigit(c))
            {
                _tokens.Add(ParseNumber(c));
                continue;
            }

            if (Grammar.Brackets.Contains(c))
            {
                _tokens.Add(ParseBracket(c));
                continue;
            }

            // Checked separately to consume (//...) and (/*...*/) comments,
            // also parses (/) division operator
            if (c == '/')
            {
                ParseFromSlash();
                continue;
            }

            if (_opStarts.Contains(c))
            {
                _tokens.Add(ParseOperator(c));
                continue;
            }

            switch (c)
            {
                case '\'':
                case '"':
                    _tokens.Add(ParseLiteralString(c));
                    break;
                case '#':
                    _comments.Add(ParseHashtagComment());
                    break;
                default:
                    _tokens.Add(ConsumeInto(new FailedToken(c), c));
                    break;
            }
        }
        return (_tokens.ToArray(), _comments.ToArray());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Token DecideOperatorType(char c, string opChar)
    {
        return c switch
        {
            '=' => new FailedToken(c),
            '!' or '~' => new UnaryOpToken(c),
            _ => new BinaryOpToken(opChar)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Token ParseOperator(char c)
    {
        // ? and : are handled earlier

        string opChar = c.ToString();
        if (_pos == _maxPos)
        {
            Token token = DecideOperatorType(c, opChar);
            return ConsumeInto(token, c);
        }

        _pos++;
        var withNextChar = opChar + charPointedAt;
        if (!Grammar.Operators.Contains(withNextChar))
        {
            return DecideOperatorType(c, opChar);
        }
        if (withNextChar != ">>" || _pos >= _maxPos)
        {
            return ConsumeInto(new BinaryOpToken(withNextChar), charPointedAt);
        }
        _pos++;
        return charPointedAt == '>'
            ? ConsumeInto(new BinaryOpToken(">>>"), '>')
            : new BinaryOpToken(withNextChar);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // consumed needed for future _pos refactor, when it will hold line/column information
    private Token ConsumeInto(Token t, char consumed)
    {
        _pos++;
        return t;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Token ConsumeInto<T>(char consumed) where T : Token, new()
    {
        _pos++;
        return new T();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Token ParseHashtagComment()
    {
        CommentToken token = new('#');
        _pos++;
        while (_pos <= _maxPos)
        {
            char c = charPointedAt;
            token.Add(c);
            _pos++;
            if (c == '\n')
            {
                break;
            }
        }
        return token;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ParseFromSlash()
    {
        BaseRecordingToken token = ConsumeIntoNew('/');
        char c = charPointedAt;
        switch (c)
        {
            case '*':
            {
                _pos++;
                while (_pos <= _maxPos && !(token.value.EndsWith("*/") && token.value.Length > 3))
                {
                    AppendInto(token, charPointedAt);
                }

                _comments.Add(new CommentToken(token));
                break;
            }
            case '/':
            {
                _pos++;
                while (_pos <= _maxPos)
                {
                    c = charPointedAt;
                    AppendInto(token, c);
                    if (c == '\n')
                    {
                        break;
                    }
                }

                _comments.Add(new CommentToken(token));
                break;
            }
            default:
                _tokens.Add(new BinaryOpToken("/"));
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Token ParseLiteralString(char c)
    {
        char openingQuote = c;
        StringToken token = new();
        AppendInto(token, openingQuote);
        while (_pos <= _maxPos)
        {
            c = charPointedAt;
            AppendInto(token, c);
            if (c == openingQuote)
            {
                break;
            }
        }

        return token;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Token ParseBracket(char bracket)
    {
        _pos++;
        return new BracketToken(bracket);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Token ParseIdentifier(char c)
    {
        IdentifierToken token = new();
        AppendInto(token, c);
        while (_pos <= _maxPos)
        {
            c = charPointedAt;
            if (!IsValidIdentifierCharacter(c))
            {
                break;
            }
            AppendInto(token, c);
        }
        var value = token.value;
        return Grammar.Keywords.Contains(value)
            ? new KeywordToken(token)
            : token;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BaseRecordingToken ConsumeIntoNew(char c)
    {
        _pos++;
        return new IdentifierToken(c);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendInto(BaseRecordingToken t, char c)
    {
        _pos++;
        t.Add(c);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Token ParseNumber(char c)
    {
        NumericToken token = new();
        var state = c == '0'
            ? NumberLexState.StartingZero
            : NumberLexState.Int;
        AppendInto(token, c);
        while (_pos <= _maxPos)
        {
            c = charPointedAt;
            switch (state)
            {
                case NumberLexState.StartingZero:
                    if (c == 'x' || c == 'X')
                    {
                        AppendInto(token, c);
                        state = NumberLexState.HexOnX;
                        continue;
                    }
                    if (!char.IsDigit(c))
                    {
                        break;
                    }
                    AppendInto(token, c);
                    state = NumberLexState.Int;
                    continue;
                case NumberLexState.Int:
                    if (char.IsDigit(c))
                    {
                        AppendInto(token, c);
                        continue;
                    }

                    if (c != '.')
                    {
                        break;
                    }
                    AppendInto(token, c);
                    state = NumberLexState.FloatOnDot;
                    continue;
                case NumberLexState.HexOnX:
                    if (!char.IsAsciiHexDigit(c))
                    {
                        break;
                    }
                    AppendInto(token, c);
                    state = NumberLexState.HexAfterX;
                    continue;
                case NumberLexState.HexAfterX:
                    if (!char.IsAsciiHexDigit(c))
                    {
                        break;
                    }
                    AppendInto(token, c);
                    continue;
                case NumberLexState.FloatOnDot:
                    if (!char.IsDigit(c))
                    {
                        break;
                    }
                    AppendInto(token, c);
                    state = NumberLexState.FloatAfterDot;
                    continue;
                case NumberLexState.FloatAfterDot:
                    if (!char.IsDigit(c))
                    {
                        break;
                    }
                    AppendInto(token, c);
                    continue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            break;
        }
        return token;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidIdentifierCharacter(char c) => c == '_' || char.IsLetterOrDigit(c);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidIdStartCharacter(char c) => c == '_' || char.IsLetter(c);

    private enum NumberLexState
    {
        StartingZero,
        Int,
        HexOnX,
        HexAfterX,
        FloatOnDot,
        FloatAfterDot
    }
}