using System.Runtime.CompilerServices;
using NMLServer.Lexing.Tokens;

namespace NMLServer.Lexing;

internal class Lexer
{
    private static readonly ISet<char> _opStarts = new HashSet<char>(Grammar.Operators.Select(o => o[0]));

    private readonly string _input;
    private readonly int _maxPos;
    private int _pos;

    private char charPointedAt => _input[_pos];

    public Lexer(string inputString)
    {
        _input = inputString;
        Token.UpdateSourceInput(_input);
        _maxPos = inputString.Length - 1;
        // Tenth part of input string seems to be a good lower-bound estimate for a number of tokens
        _tokens = new List<Token>(inputString.Length / 10);
    }

    private readonly List<Token> _tokens;
    private readonly List<Token> _comments = new();

    public (Token[] tokens, Token[] comments) Tokenize()
    {
        var span = _input.AsSpan();
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
                case '=':
                    _tokens.Add(new AssignmentToken(_pos++));
                    continue;

                case ';':
                    _tokens.Add(new SemicolonToken(_pos++));
                    continue;

                case ':':
                    _tokens.Add(new ColonToken(_pos++));
                    continue;

                case '?':
                    _tokens.Add(new TernaryOpToken(_pos++));
                    continue;
            }

            if (IsValidIdStartCharacter(c))
            {
                _tokens.Add(ParseIdentifier(c, span));
                continue;
            }

            if (char.IsDigit(c))
            {
                _tokens.Add(ParseNumber(c));
                continue;
            }

            if (c == '/')
            {
                ParseFromSlash();
                continue;
            }

            if (Grammar.Brackets.Contains(c))
            {
                _tokens.Add(new BracketToken(_pos++));
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
                    _tokens.Add(new FailedToken(_pos));
                    _pos++;
                    break;
            }
        }
        return (_tokens.ToArray(), _comments.ToArray());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Token DecideOperatorType(char c, int start, int end)
    {
        return c switch
        {
            '=' => new FailedToken(start),
            '!' or '~' => new UnaryOpToken(start),
            _ => new BinaryOpToken(start, end, c)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Token ParseOperator(char c)
    {
        // ? and : are handled earlier
        string opChar = c.ToString();
        int start = _pos;
        if (_pos == _maxPos)
        {
            Token token = DecideOperatorType(c, start, _pos);
            _pos++;
            return token;
        }

        _pos++;
        var withNextChar = opChar + charPointedAt;
        if (!Grammar.Operators.Contains(withNextChar))
        {
            return DecideOperatorType(c, start, _pos);
        }
        if (withNextChar != ">>" || _pos >= _maxPos)
        {
            return new BinaryOpToken(start, ++_pos, withNextChar);
        }
        _pos++;
        return charPointedAt == '>'
            ? new BinaryOpToken(start, ++_pos, charPointedAt)
            : new BinaryOpToken(start, _pos, withNextChar);
    }

    private Token ParseHashtagComment()
    {
        int start = _pos++;
        while (_pos <= _maxPos)
        {
            char c = charPointedAt;
            if (c == '\n')
            {
                break;
            }
            _pos++;
        }
        return new CommentToken(start, _pos);
    }

    private void ParseFromSlash()
    {
        int start = _pos++;
        switch (charPointedAt)
        {
            case '*':
                _pos++;
                while (_pos <= _maxPos && !(_pos - start > 3 && _input[(_pos - 2).._pos].EndsWith("*/")))
                {
                    _pos++;
                }
                _comments.Add(new CommentToken(start, _pos));
                break;

            case '/':
                _pos++;
                while (_pos <= _maxPos)
                {
                    var c = charPointedAt;
                    _pos++;
                    if (c == '\n')
                    {
                        break;
                    }
                }

                _comments.Add(new CommentToken(start, _pos));
                break;

            default:
                _tokens.Add(new BinaryOpToken(start, _pos, '/'));
                break;
        }
    }

    private Token ParseLiteralString(char openingQuote)
    {
        int start = _pos++;
        while (_pos <= _maxPos)
        {
            char c = charPointedAt;
            _pos++;
            if (c == openingQuote)
            {
                break;
            }
        }

        return new StringToken(start, _pos);
    }

    private Token ParseIdentifier(char first, ReadOnlySpan<char> span)
    {
        int start = _pos++;
        char c = '\0';
        while (_pos <= _maxPos)
        {
            c = charPointedAt;
            if (!IsValidIdentifierCharacter(c))
            {
                break;
            }
            _pos++;
        }
        switch (_pos - start)
        {
            case 1 when c == 'm':
                switch (_maxPos - _pos)
                {
                    case 2 when span[_pos..(_pos + 2)] == "/s":
                    case > 2 when span[_pos..(_pos + 2)] == "/s" && !IsValidIdentifierCharacter(span[_pos + 2]):
                    {
                        _pos += 2;
                        return new UnitToken(start, UnitType.MPS);
                    }
                }
                break;

            case 2 when span[start] == 'k' && c == 'm':
            {
                switch (_maxPos - _pos)
                {
                    case 2 when span[_pos..(_pos + 2)] == "/h":
                    case > 2 when span[_pos..(_pos + 2)] == "/h" && !IsValidIdentifierCharacter(span[_pos + 2]):
                    {
                        _pos += 2;
                        return new UnitToken(start, UnitType.KMPH);
                    }
                }
                break;
            }
        }
        var value = _input[start.._pos];
        if (Grammar.Units.ContainsKey(value))
        {
            return new UnitToken(start, value);
        }
        if (Grammar.KeywordTypeByString.TryGetValue(value, out var keywordType))
        {
            return new KeywordToken(start, _pos, keywordType);
        }
        return new IdentifierToken(start, _pos);
    }

    private Token ParseNumber(char c)
    {
        var state = c == '0'
            ? NumberLexState.StartingZero
            : NumberLexState.Int;

        int start = _pos++;
        while (_pos <= _maxPos)
        {
            c = charPointedAt;
            switch (state)
            {
                case NumberLexState.StartingZero:
                    if (c == 'x' || c == 'X')
                    {
                        _pos++;
                        state = NumberLexState.HexOnX;
                        continue;
                    }
                    if (!char.IsDigit(c))
                    {
                        break;
                    }
                    _pos++;
                    state = NumberLexState.Int;
                    continue;

                case NumberLexState.Int:
                    if (char.IsDigit(c))
                    {
                        _pos++;
                        continue;
                    }

                    if (c != '.')
                    {
                        break;
                    }
                    _pos++;
                    state = NumberLexState.FloatOnDot;
                    continue;

                case NumberLexState.HexOnX:
                    if (!char.IsAsciiHexDigit(c))
                    {
                        break;
                    }
                    _pos++;
                    state = NumberLexState.HexAfterX;
                    continue;

                case NumberLexState.HexAfterX:
                    if (!char.IsAsciiHexDigit(c))
                    {
                        break;
                    }
                    _pos++;
                    continue;

                case NumberLexState.FloatOnDot:
                    if (!char.IsDigit(c))
                    {
                        break;
                    }
                    _pos++;
                    state = NumberLexState.FloatAfterDot;
                    continue;

                case NumberLexState.FloatAfterDot:
                    if (!char.IsDigit(c))
                    {
                        break;
                    }
                    _pos++;
                    continue;

                default:
                    throw new Exception();
            }
            break;
        }
        return new NumericToken(start, _pos);
    }

    private static bool IsValidIdentifierCharacter(char c) => c == '_' || char.IsLetterOrDigit(c);

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