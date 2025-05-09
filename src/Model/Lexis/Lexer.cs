using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NMLServer.Model.Lexis;

// TODO docstrings
internal ref struct Lexer(StringView content, List<int> lineLengths, int pos = 0, int lineLength = 0)
{
    private int _pos = pos;
    private int _lineStart = pos - lineLength;
    private readonly StringView _content = content;
    private readonly int _maxPos = content.Length - 1;

    private readonly char currentChar => FastAccess(_content, _pos);
    private readonly char nextChar => FastAccess(_content, _pos + 1);
    private readonly bool isAtLastChar => _pos == _maxPos;
    private readonly bool isInBounds => _pos <= _maxPos;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T FastAccess<T>(ReadOnlySpan<T> view, nint offset)
        => Unsafe.Add(ref MemoryMarshal.GetReference(view), offset);

    public void ProcessUntilFileEnd(in List<Token> tokens)
    {
        while (LexToken(out _) is { } token)
            tokens.Add(token);
    }

    /// <summary>Try to lex a token at current Lexer state in source code span.</summary>
    /// <param name="tokenStart">
    /// The start position of token is returned to avoid possible virtual call in outer scope.
    /// </param>
    /// <returns>The lexed token, if any; null otherwise.</returns>
    /// <remarks>
    /// This method should not be called after previous call returned null, as currently calculated line length is added
    /// to <see cref="lineLengths"/> list in that case.
    /// </remarks>
    public Token? LexToken(out int tokenStart)
    {
        while (isInBounds)
        {
            char c = currentChar;
            if (c is '\n')
            {
                AddLine();
                continue;
            }
            if (char.IsWhiteSpace(c))
            {
                ++_pos;
                continue;
            }
            tokenStart = _pos;
            if (char.IsLetter(c) || c is '_')
            {
                return ParseIdentifier();
            }
            return c switch
            {
                ';' => new SemicolonToken(_pos++),
                ':' => new ColonToken(_pos++),
                '?' => new TernaryOpToken(_pos++),
                '[' or ']' or '(' or ')' or '{' or '}' => new BracketToken(_pos++, c),
                '~' => new UnaryOpToken(_pos++, c),

                ',' => new BinaryOpToken(_pos, ++_pos, OperatorType.Comma),
                '^' => new BinaryOpToken(_pos, ++_pos, OperatorType.BinaryXor),
                '+' => new BinaryOpToken(_pos, ++_pos, OperatorType.Plus),
                '-' => new BinaryOpToken(_pos, ++_pos, OperatorType.Minus),
                '*' => new BinaryOpToken(_pos, ++_pos, OperatorType.Multiply),
                '%' => new BinaryOpToken(_pos, ++_pos, OperatorType.Modula),

                '|' => TryParseOperator(OperatorType.BinaryOr),
                '&' => TryParseOperator(OperatorType.BinaryAnd),
                '<' => TryParseOperator(OperatorType.Lt),
                '>' => TryParseOperator(OperatorType.Gt),
                '!' => TryParseOperator(OperatorType.LogicalNot),

                >= '0' and <= '9' => ParseNumber(c),
                '/' => ParseFromSlash(),
                '=' => ParseOnEqualsSign(),
                '.' => ParseOnDot(),
                '\'' or '"' => ParseLiteralString(c),
                '#' => ParseHashtagComment(),

                _ => new UnknownToken(_pos++)
            };
        }
        CompleteLine();
        tokenStart = -1;
        return null;
    }

    private Token ParseOnDot()
    {
        if (isAtLastChar || nextChar is not '.')
        {
            return new UnknownToken(_pos++);
        }
        RangeToken result = new(_pos);
        _pos += 2;
        return result;
    }

    private Token ParseOnEqualsSign()
    {
        return isAtLastChar || nextChar is not '='
            ? new AssignmentToken(_pos++)
            : new BinaryOpToken(_pos, _pos += 2, OperatorType.Eq);
    }

    private BinaryOpToken TryParseOperator(OperatorType type)
    {
        if (isAtLastChar)
        {
            return new BinaryOpToken(_pos, ++_pos, type);
        }
        var start = _pos++;
        switch (currentChar)
        {
            case '|' when type is OperatorType.BinaryOr:
                type = OperatorType.LogicalOr;
                break;

            case '&' when type is OperatorType.BinaryAnd:
                type = OperatorType.LogicalAnd;
                break;

            case '=':
                var nextType = type switch
                {
                    OperatorType.Lt => OperatorType.Le,
                    OperatorType.Gt => OperatorType.Ge,
                    OperatorType.LogicalNot => OperatorType.Ne,
                    _ => OperatorType.None
                };
                if (nextType is OperatorType.None)
                {
                    goto label_Return;
                }
                type = nextType;
                break;

            case '<' when type is OperatorType.Lt:
                type = OperatorType.ShiftRight;
                break;

            case '>' when type is OperatorType.Gt:
                if (isAtLastChar || nextChar is not '>')
                {
                    type = OperatorType.ShiftRight;
                    break;
                }
                type = OperatorType.ShiftRightFunky;
                _pos += 2;
                goto label_Return;

            default:
                goto label_Return;
        }
        ++_pos;

        label_Return:
        return new BinaryOpToken(start, _pos, type);
    }

    private CommentToken ParseHashtagComment()
    {
        int start = _pos++;
        while (isInBounds && currentChar is not '\n')
        {
            ++_pos;
        }
        return new CommentToken(start, _pos);
    }

    private Token ParseFromSlash()
    {
        if (isAtLastChar)
        {
            return new BinaryOpToken(_pos, ++_pos, OperatorType.Divide);
        }
        int start = _pos++;
        switch (currentChar)
        {
            case '*':
                ++_pos;
                while (isInBounds)
                {
                    switch (currentChar)
                    {
                        case '/' when _pos - start >= 3 && FastAccess(_content, _pos - 1) is '*':
                            break;

                        case '\n':
                            AddLine();
                            continue;

                        default:
                            ++_pos;
                            continue;
                    }
                    ++_pos;
                    break;
                }
                return new CommentToken(start, _pos);

            case '/':
                do
                {
                    ++_pos;
                }
                while (isInBounds && currentChar is not '\n');
                return new CommentToken(start, _pos);

            default:
                return new BinaryOpToken(start, _pos, OperatorType.Divide);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddLine()
    {
        ++_pos;
        lineLengths.Add(_pos - _lineStart);
        _lineStart = _pos;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CompleteLine() => lineLengths.Add(_pos - _lineStart);

    private StringToken ParseLiteralString(char openingQuote)
    {
        int start = _pos++;
        while (isInBounds)
        {
            char c = currentChar;
            if (c is '\n')
            {
                break;
            }
            ++_pos;
            if (c == openingQuote)
            {
                break;
            }
        }
        return new StringToken(start, _pos);
    }

    private Token ParseIdentifier()
    {
        int start = _pos++;
        char current = '\0';
        for (; isInBounds; ++_pos)
        {
            current = currentChar;
            if (!IsIdentifierChar(current))
            {
                break;
            }
        }
        var value = _content[start.._pos];
        var charsLeft = _maxPos - _pos;
        switch (current)
        {
            case '/' when charsLeft == 1
                          || (charsLeft > 1 && !IsIdentifierChar(FastAccess(_content, _pos + 2))):
                var next = nextChar;
                UnitType unitType = value switch
                {
                    "m" when next is 's' => UnitType.MPS,
                    "km" when next is 'h' => UnitType.KMPH,
                    _ => UnitType.None
                };
                if (unitType is UnitType.None)
                    break;
                _pos += 2;
                return new UnitToken(start, unitType);

            case '%' when value is "snow":
                ++_pos;
                return new UnitToken(start, UnitType.Snow);
        }

        if (Grammar.UnitLiterals.TryGetValue(value, out var unit))
            return new UnitToken(start, unit);

        if (Grammar.Keywords.TryGetValue(value, out var data))
            return new KeywordToken(start, _pos, data.type, data.kind);

        var (kind, hash) = Grammar.GetSymbolInfo(value);
        return new IdentifierToken(start, _pos, kind, hash);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIdentifierChar(char c) => char.IsAsciiLetterOrDigit(c) | c is '_';

    private NumericToken ParseNumber(char c)
    {
        const int startingZero = 0;
        const int onInt = 1;
        const int hexOnX = 2;
        const int hexAfterX = 3;
        const int floatOnDot = 4;
        const int floatAfterDot = 5;

        var innerState = c is '0' ? startingZero : onInt;
        int start = _pos++;
        for (; isInBounds; ++_pos)
        {
            c = currentChar;
            var curIsDigit = (c >= '0') & (c <= '9');
            switch (innerState)
            {
                case startingZero when c is 'x' or 'X':
                    innerState = hexOnX;
                    continue;

                case startingZero when c is '.':
                case onInt when c is '.':
                    innerState = floatOnDot;
                    continue;

                case startingZero when curIsDigit:
                    innerState = onInt;
                    continue;

                case hexOnX when char.IsAsciiHexDigit(c):
                    innerState = hexAfterX;
                    continue;

                case floatOnDot when c is '.':
                    --_pos;
                    goto label_End;

                case floatOnDot when curIsDigit:
                    innerState = floatAfterDot;
                    continue;

                case onInt when curIsDigit:
                case hexAfterX when char.IsAsciiHexDigit(c):
                case floatAfterDot when char.IsDigit(c):
                    continue;

                default:
                    goto label_End;
            }
        }
        label_End:
        return new NumericToken(start, _pos);
    }
}