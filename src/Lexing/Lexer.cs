using System.Runtime.CompilerServices;
using NMLServer.Lexing.Tokens;

namespace NMLServer.Lexing;

internal ref struct Lexer
{
    private static readonly HashSet<char> _opStarts = new(
        from
            op in Grammar.Operators
        select
            op[0]
    );
    private readonly ReadOnlySpan<char> _context;
    private readonly int _maxPos;
    private int _pos;

    public Lexer(ReadOnlySpan<char> input)
    {
        _context = input;
        _maxPos = input.Length - 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref readonly char GetCurrentChar() => ref _context[_pos];



    public (IReadOnlyList<Token> tokens, List<int> lineLengths) Process()
    {
        List<Token> tokens = new();
        List<int> lineLengths = new();
        int lineStart = 0;
        while (_pos <= _maxPos)
        {
            char c = GetCurrentChar();
            if (c is '\n')
            {
                lineLengths.Add(_pos - lineStart);
                lineStart = ++_pos;
                continue;
            }
            if (char.IsWhiteSpace(c))
            {
                ++_pos;
                continue;
            }
            switch (c)
            {
                case '=':
                    if (_pos == _maxPos || _context[_pos + 1] is not '=')
                    {
                        tokens.Add(new AssignmentToken(_pos++));
                        continue;
                    }
                    tokens.Add(new BinaryOpToken(_pos, ++_pos, "=="));
                    ++_pos;
                    continue;

                case '.':
                    if (_pos == _maxPos)
                    {
                        break;
                    }
                    ++_pos;
                    tokens.Add(GetCurrentChar() is '.'
                        ? new RangeToken(_pos++ - 1)
                        : new FailedToken(_pos - 1)
                    );
                    continue;

                case ';':
                    tokens.Add(new SemicolonToken(_pos++));
                    continue;

                case ':':
                    tokens.Add(new ColonToken(_pos++));
                    continue;

                case '?':
                    tokens.Add(new TernaryOpToken(_pos++));
                    continue;
            }

            if (IsValidIdStartCharacter(c))
            {
                tokens.Add(ParseIdentifier(c));
                continue;
            }

            if (char.IsDigit(c))
            {
                tokens.Add(ParseNumber(c));
                continue;
            }

            if (c == '/')
            {
                tokens.Add(ParseFromSlash(lineLengths, ref lineStart));
                continue;
            }

            if (Grammar.Brackets.Contains(c))
            {
                tokens.Add(new BracketToken(_pos++, c));
                continue;
            }

            if (_opStarts.Contains(c))
            {
                tokens.Add(ParseOperator(c));
                continue;
            }

            switch (c)
            {
                case '\'':
                case '"':
                    tokens.Add(ParseLiteralString(c));
                    break;

                case '#':
                    tokens.Add(ParseHashtagComment());
                    break;

                default:
                    tokens.Add(new FailedToken(_pos++));
                    break;
            }
        }
        lineLengths.Add(_pos - lineStart);
        return (tokens.ToArray(), lineLengths);
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

    private Token ParseFromSlash(ICollection<int> lineLengths, ref int lineStart)
    {
        int start = _pos++;
        if (_pos > _maxPos)
        {
            return new BinaryOpToken(start, _pos, '/');
        }
        switch (GetCurrentChar())
        {
            case '*':
                ++_pos;
                while (_pos <= _maxPos && _pos - start < 3)
                {
                    if (_context[_pos] is '\n')
                    {
                        lineLengths.Add(_pos - lineStart);
                        lineStart = ++_pos;
                        continue;
                    }
                    ++_pos;
                }
                char prev = _context[_pos - 1];
                while (_pos <= _maxPos)
                {
                    var current = GetCurrentChar();
                    if (current is '\n')
                    {
                        prev = '\n';
                        lineLengths.Add(_pos - lineStart);
                        lineStart = ++_pos;
                        continue;
                    }
                    if (prev is '*' && current is '/')
                    {
                        break;
                    }
                    prev = current;
                    ++_pos;
                }
                return new CommentToken(start, ++_pos);

            case '/':
                ++_pos;
                while (_pos <= _maxPos)
                {
                    if (GetCurrentChar() is '\n')
                    {
                        break;
                    }
                    ++_pos;
                }
                return new CommentToken(start, ++_pos);

            default:
                return new BinaryOpToken(start, _pos, '/');
        }
    }

    private Token ParseLiteralString(char openingQuote)
    {
        int start = _pos++;
        while (_pos <= _maxPos)
        {
            char c = GetCurrentChar();
            ++_pos;
            if (c == openingQuote || c is '\n')
            {
                break;
            }
        }
        return new StringToken(start, _pos);
    }

    private Token ParseIdentifier(char first)
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
            && (_maxPos - _pos == 1 || (_maxPos - _pos > 1 && !IsValidIdentifierCharacter(_context[_pos + 2]))))
        {
            switch (_pos - start)
            {
                case 1 when first is 'm' && _context[_pos + 1] is 's':
                    _pos += 2;
                    return new UnitToken(start, UnitType.MPS, 3);

                case 2 when first is 'k' && _context[_pos - 1] is 'm' && _context[_pos + 1] is 'h':
                    _pos += 2;
                    return new UnitToken(start, UnitType.KMPH, 4);
            }
        }
        var value = _context[start.._pos];
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
                    if (c is '.')
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