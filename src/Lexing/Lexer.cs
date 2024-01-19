using System.Runtime.CompilerServices;
using NMLServer.Lexing.Tokens;

namespace NMLServer.Lexing;

internal class Lexer
{
    private static readonly HashSet<char> _opStarts = new(from op in Grammar.Operators select op[0]);

    private readonly string _input;
    private readonly int _maxPos;
    private int _pos;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private char GetCurrentChar() => _input[_pos];

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

    public (Token[] tokens, Token[] comments) Process()
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
                    _pos++;
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
                ParseFromSlash(span);
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

    private static Token DecideOperatorType(char c, int start, int end)
    {
        return c switch
        {
            '=' => new FailedToken(start),
            '!' or '~' => new UnaryOpToken(start),
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
            Token token = DecideOperatorType(c, start, _pos);
            _pos++;
            return token;
        }

        _pos++;
        var withNextChar = opChar + GetCurrentChar();
        if (!Grammar.Operators.Contains(withNextChar))
        {
            return DecideOperatorType(c, start, _pos);
        }
        if (withNextChar != ">>" || _pos >= _maxPos)
        {
            return new BinaryOpToken(start, ++_pos, withNextChar);
        }
        _pos++;
        return GetCurrentChar() == '>'
            ? new BinaryOpToken(start, ++_pos, GetCurrentChar())
            : new BinaryOpToken(start, _pos, withNextChar);
    }

    private Token ParseHashtagComment()
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

    private void ParseFromSlash(ReadOnlySpan<char> span)
    {
        int start = _pos++;
        switch (GetCurrentChar())
        {
            // TODO: use buffering for checking equality of last two characters to "*/" sequence
            case '*':
                _pos++;
                while (_pos <= _maxPos
                       && (_pos - start <= 3 || !span[(_pos - 2).._pos].Equals("*/", StringComparison.Ordinal)))
                {
                    _pos++;
                }
                _comments.Add(new CommentToken(start, _pos));
                break;

            case '/':
                _pos++;
                while (_pos <= _maxPos)
                {
                    var c = GetCurrentChar();
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
        for (; _pos <= _maxPos; ++_pos)
        {
            if (!IsValidIdentifierCharacter(GetCurrentChar()))
            {
                break;
            }
        }
        if (GetCurrentChar() is '/')
        {
            switch (_maxPos - _pos)
            {
                case 2:
                    switch (_pos - start)
                    {
                        case 1 when first is 'm' && _input[_pos + 1] is 's':
                            _pos += 2;
                            return new UnitToken(start, UnitType.MPS);

                        case 2 when first is 'k' && _input[_pos - 1] is 'm' && _input[_pos + 1] is 'h':
                            _pos += 2;
                            return new UnitToken(start, UnitType.KMPH);
                    }
                    break;

                case >= 2:
                    switch (_pos - start)
                    {
                        case 1 when first is 'm' && _input[_pos + 1] is 's'
                                                 && !IsValidIdentifierCharacter(_input[_pos + 2]):
                            _pos += 2;
                            return new UnitToken(start, UnitType.MPS);

                        case 2 when first is 'k' && _input[_pos - 1] is 'm' && _input[_pos + 1] is 'h'
                                    && !IsValidIdentifierCharacter(_input[_pos + 2]):

                            _pos += 2;
                            return new UnitToken(start, UnitType.KMPH);
                    }
                    break;
            }
        }

        Span<char> value = stackalloc char[_pos - start];
        view[start.._pos].CopyTo(value);

        if (_pos - start <= 4 && Grammar.IsUnit(value, out var type))
        {
            return new UnitToken(start, type);
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