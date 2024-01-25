using System.Runtime.CompilerServices;
using NMLServer.Lexing.Tokens;

namespace NMLServer.Lexing;

internal class Lexer
{
    private static readonly HashSet<char> _opStarts = new(
        from
            op in Grammar.Operators
        select
            op[0]
    );

    private readonly string _input;
    private readonly int _maxPos;
    private int _pos;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private char GetCurrentChar() => _input[_pos];

    public Lexer(string inputString)
    {
        _input = inputString;
        _maxPos = inputString.Length - 1;
        _tokens = new List<Token>(inputString.Length / 10);
    }

    private readonly List<Token> _tokens;
    private readonly List<CommentToken> _comments = new();

    public (Token[] tokens, CommentToken[] comments) Process()
    {
        var span = _input.AsSpan();
        while (_pos <= _maxPos)
        {
            char c = GetCurrentChar();
            if (char.IsWhiteSpace(c))
            {
                _pos++;
                continue;
            }
            switch (c)
            {
                case '=':
                    if (_pos == _maxPos || _input[_pos + 1] is not '=')
                    {
                        _tokens.Add(new AssignmentToken(_pos++));
                        continue;
                    }
                    _tokens.Add(new BinaryOpToken(_pos, ++_pos, "=="));
                    ++_pos;
                    continue;

                case '.':
                    if (_pos == _maxPos)
                    {
                        break;
                    }
                    ++_pos;
                    _tokens.Add(GetCurrentChar() is '.'
                        ? new RangeToken(_pos++ - 1)
                        : new FailedToken(_pos - 1)
                    );
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
                _tokens.Add(new BracketToken(_pos++, c));
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
                    _tokens.Add(new FailedToken(_pos++));
                    break;
            }
        }
        return (_tokens.ToArray(), _comments.ToArray());
    }

    private static Token DecideSingleCharacterOperatorType(char c, int start, int end)
    {
        return c switch
        {
            '=' => new FailedToken(start),
            '!' or '~' => new UnaryOpToken(start, c),
            _ => new BinaryOpToken(start, end, c)
        };
    }

    private Token ParseOperator(char c)
    {
        // ? and : are handled earlier
        string opChar = c.ToString();
        int start = _pos;
        if (_pos == _maxPos)
        {
            Token token = DecideSingleCharacterOperatorType(c, start, _pos);
            ++_pos;
            return token;
        }

        ++_pos;
        var withNextChar = opChar + GetCurrentChar();
        if (!Grammar.Operators.Contains(withNextChar))
        {
            return DecideSingleCharacterOperatorType(c, start, _pos);
        }
        if (withNextChar != ">>" || _pos >= _maxPos)
        {
            return new BinaryOpToken(start, ++_pos, withNextChar);
        }
        ++_pos;
        return GetCurrentChar() is '>'
            ? new BinaryOpToken(start, ++_pos, '>')
            : new BinaryOpToken(start, _pos, withNextChar);
    }

    private CommentToken ParseHashtagComment()
    {
        int start = _pos++;
        while (_pos <= _maxPos)
        {
            char c = GetCurrentChar();
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
        if (_pos > _maxPos)
        {
            _tokens.Add(new BinaryOpToken(start, _pos, '/'));
            return;
        }
        switch (GetCurrentChar())
        {
            case '*':
                ++_pos;
                while (_pos <= _maxPos && _pos - start < 3)
                {
                    ++_pos;
                }
                char prev = _input[_pos - 1];
                while (_pos <= _maxPos)
                {
                    var current = GetCurrentChar();
                    if (prev is '*' && current is '/')
                    {
                        break;
                    }
                    prev = current;
                    ++_pos;
                }
                _comments.Add(new CommentToken(start, ++_pos));
                break;

            case '/':
                ++_pos;
                while (_pos <= _maxPos)
                {
                    var c = GetCurrentChar();
                    if (c == '\n')
                    {
                        break;
                    }
                    ++_pos;
                }
                _comments.Add(new CommentToken(start, ++_pos));
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
            char c = GetCurrentChar();
            _pos++;
            if (c == openingQuote)
            {
                break;
            }
        }
        return new StringToken(start, _pos);
    }

    private Token ParseIdentifier(char first, ReadOnlySpan<char> view)
    {
        int start = _pos++;
        char current = '\0';
        for (; _pos <= _maxPos; ++_pos)
        {
            current = GetCurrentChar();
            if (!IsValidIdentifierCharacter(current))
            {
                break;
            }
        }
        if (current is '/'
            && (_maxPos - _pos == 1 || (_maxPos - _pos > 1 && !IsValidIdentifierCharacter(_input[_pos + 2]))))
        {
            switch (_pos - start)
            {
                case 1 when first is 'm' && _input[_pos + 1] is 's':
                    _pos += 2;
                    return new UnitToken(start, UnitType.MPS, 3);

                case 2 when first is 'k' && _input[_pos - 1] is 'm' && _input[_pos + 1] is 'h':
                    _pos += 2;
                    return new UnitToken(start, UnitType.KMPH, 4);
            }
        }
        var value = view[start.._pos];
        if (current is '%' && _pos - start == 4 && value.Equals("snow", StringComparison.Ordinal))
        {
            _pos += 1;
            return new UnitToken(start, UnitType.Snow, 5);
        }

        if (_pos - start <= 4 && UnitToken.IsLiteralUnit(value, out var result))
        {
            return new UnitToken(start, result.type, result.length);
        }
        if (Grammar.KeywordTypeByString.TryGetValue(new string(value), out var keywordType))
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
        for (; _pos <= _maxPos; ++_pos)
        {
            c = GetCurrentChar();
            switch (state)
            {
                case NumberLexState.StartingZero:
                    switch (c)
                    {
                        case 'x':
                        case 'X':
                            state = NumberLexState.HexOnX;
                            continue;

                        case '.':
                            state = NumberLexState.FloatOnDot;
                            continue;
                    }
                    if (!char.IsDigit(c))
                    {
                        return new NumericToken(start, _pos);
                    }
                    state = NumberLexState.Int;
                    continue;

                case NumberLexState.Int:
                    if (char.IsDigit(c))
                    {
                        continue;
                    }
                    if (c != '.')
                    {
                        return new NumericToken(start, _pos);
                    }
                    state = NumberLexState.FloatOnDot;
                    continue;

                case NumberLexState.HexOnX:
                    if (!char.IsAsciiHexDigit(c))
                    {
                        return new NumericToken(start, _pos);
                    }
                    state = NumberLexState.HexAfterX;
                    continue;

                case NumberLexState.HexAfterX:
                    if (!char.IsAsciiHexDigit(c))
                    {
                        return new NumericToken(start, _pos);
                    }
                    continue;

                case NumberLexState.FloatOnDot:
                    // range operator
                    if (c == '.')
                    {
                        return new NumericToken(start, --_pos);
                    }
                    if (!char.IsDigit(c))
                    {
                        return new NumericToken(start, _pos);
                    }
                    state = NumberLexState.FloatAfterDot;
                    continue;

                case NumberLexState.FloatAfterDot:
                    if (!char.IsDigit(c))
                    {
                        return new NumericToken(start, _pos);
                    }
                    continue;
            }
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