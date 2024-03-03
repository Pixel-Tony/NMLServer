using System.Runtime.CompilerServices;

namespace NMLServer.Lexing;

// TODO docstrings
internal ref struct Lexer(StringView content, in List<int> lineLengths, int pos, int end, int lineLength = 0)
{
    private int _lineStart = pos - lineLength;
    private readonly StringView _content = content;
    private readonly List<int> _lineLengths = lineLengths;
    private readonly int _maxPos = end - 1;

    private readonly char currentChar => _content[pos];
    private readonly char nextChar => _content[pos + 1];
    private readonly bool isAtLastChar => pos == _maxPos;
    private readonly bool isInBounds => pos <= _maxPos;

    public Lexer(StringView content, in List<int> lineLengths) : this(content, in lineLengths, 0, content.Length)
    { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessUntilFileEnd(in List<Token> tokens)
    {
        for (var token = LexToken(out _); token is not null; token = LexToken(out _))
        {
            tokens.Add(token);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessUntilFileEnd(ref Token? token, in List<Token> tokens)
    {
        for (token = LexToken(out _); token is not null; token = LexToken(out _))
        {
            tokens.Add(token);
        }
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
                ++pos;
                continue;
            }
            tokenStart = pos;
            if (char.IsLetter(c) || c is '_')
            {
                return ParseIdentifier();
            }
            return c switch
            {
                ';' => new SemicolonToken(pos++),
                ':' => new ColonToken(pos++),
                '?' => new TernaryOpToken(pos++),
                '[' or ']' or '(' or ')' or '{' or '}'
                    => new BracketToken(pos++, c),
                >= '0' and <= '9' => ParseNumber(c),
                '=' => ParseOnEqualsSign(),
                '.' => ParseOnDot(),
                '/' => ParseFromSlash(),
                '\'' or '"' => ParseLiteralString(c),
                '#' => ParseHashtagComment(),
                _ => TryParseOperator(c, Grammar.GetOperatorType(c))
            };
        }
        Complete();
        tokenStart = -1;
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Token ParseOnDot()
    {
        if (isAtLastChar)
        {
            return new UnknownToken(pos++);
        }
        ++pos;
        return currentChar is '.'
            ? new RangeToken(pos++ - 1)
            : new UnknownToken(pos - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Token ParseOnEqualsSign()
    {
        if (isAtLastChar || nextChar is not '=')
        {
            return new AssignmentToken(pos++);
        }
        var result = new BinaryOpToken(pos, ++pos, OperatorType.Eq);
        ++pos;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Token TryParseOperator(char c, OperatorType type)
    {
        if (type is OperatorType.None)
        {
            return new UnknownToken(pos++);
        }
        int start = pos++;
        if (isAtLastChar)
        {
            return c switch
            {
                '=' => new UnknownToken(start),
                '!' or '~' => new UnaryOpToken(start, c),
                _ => new BinaryOpToken(start, pos, type)
            };
        }
        StringView needle = stackalloc char[2] { c, currentChar };
        var opTokenType = Grammar.GetOperatorType(needle);
        if (opTokenType is OperatorType.None)
        {
            return c switch
            {
                '=' => new UnknownToken(start),
                '!' or '~' => new UnaryOpToken(start, c),
                _ => new BinaryOpToken(start, pos, type)
            };
        }
        if (opTokenType is not OperatorType.ShiftRight || isAtLastChar)
        {
            return new BinaryOpToken(start, ++pos, opTokenType);
        }
        ++pos;
        return currentChar is '>'
            ? new BinaryOpToken(start, ++pos, OperatorType.ShiftRightFunky)
            : new BinaryOpToken(start, pos, opTokenType);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private CommentToken ParseHashtagComment()
    {
        int start = pos++;
        while (isInBounds)
        {
            char c = currentChar;
            if (c == '\n')
            {
                break;
            }
            pos++;
        }
        return new CommentToken(start, pos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Token ParseFromSlash()
    {
        if (isAtLastChar)
        {
            return new BinaryOpToken(pos, ++pos, OperatorType.Divide);
        }
        int start = pos++;
        switch (currentChar)
        {
            case '*':
                ++pos;
                while (isInBounds && pos - start < 3)
                {
                    if (currentChar is '\n')
                    {
                        AddLine();
                        continue;
                    }
                    ++pos;
                }
                char prev = _content[pos - 1];
                while (isInBounds)
                {
                    var current = currentChar;
                    if (current is '\n')
                    {
                        prev = '\n';
                        AddLine();
                        continue;
                    }
                    if (prev is '*' && current is '/')
                    {
                        break;
                    }
                    prev = current;
                    ++pos;
                }
                return new CommentToken(start, ++pos);

            case '/':
                ++pos;
                while (isInBounds && currentChar is not '\n')
                {
                    ++pos;
                }
                return new CommentToken(start, pos);

            default:
                return new BinaryOpToken(start, pos, OperatorType.Divide);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddLine()
    {
        ++pos;
        _lineLengths.Add(pos - _lineStart);
        _lineStart = pos;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Complete() => _lineLengths.Add(pos - _lineStart);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private StringToken ParseLiteralString(char openingQuote)
    {
        int start = pos++;
        while (isInBounds)
        {
            char c = currentChar;
            if (c == openingQuote)
            {
                ++pos;
                break;
            }
            if (c is '\n')
            {
                break;
            }
            ++pos;
        }
        return new StringToken(start, pos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Token ParseIdentifier()
    {
        int start = pos++;
        char current = '\0';
        for (; isInBounds; ++pos)
        {
            current = currentChar;
            if (!current.IsValidIdentifierCharacter())
            {
                break;
            }
        }
        var value = _content[start..pos];
        switch (current)
        {
            case '/' when _maxPos - pos == 1:
            case '/' when _maxPos - pos > 1 && !_content[pos + 2].IsValidIdentifierCharacter():
                switch (pos - start)
                {
                    case 1 when value is "m" && nextChar is 's':
                        pos += 2;
                        return new UnitToken(start, UnitType.MPS);

                    case 2 when value is "km" && nextChar is 'h':
                        pos += 2;
                        return new UnitToken(start, UnitType.KMPH);
                }
                break;

            case '%' when value is "snow":
                ++pos;
                return new UnitToken(start, UnitType.Snow);
        }

        for (var unitType = CheckLiteralUnit(value); unitType is not UnitType.None;)
        {
            return new UnitToken(start, unitType);
        }
        for (var (keywordType, kind) = CheckKeyword(value); keywordType is not KeywordType.None;)
        {
            return new KeywordToken(start, pos, keywordType, kind);
        }
        return new IdentifierToken(start, pos, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private NumericToken ParseNumber(char c)
    {
        const byte startingZero = 0;
        const byte onInt = 1;
        const byte hexOnX = 2;
        const byte hexAfterX = 3;
        const byte floatOnDot = 4;
        const byte floatAfterDot = 5;

        var innerState = c is '0'
            ? startingZero
            : onInt;
        int start = pos++;
        for (; isInBounds; ++pos)
        {
            c = currentChar;
            switch (innerState)
            {
                case startingZero:
                    switch (c)
                    {
                        case 'x':
                        case 'X':
                            innerState = hexOnX;
                            continue;

                        case '.':
                            innerState = floatOnDot;
                            continue;
                    }
                    if (!char.IsDigit(c))
                    {
                        goto label_End;
                    }
                    innerState = onInt;
                    continue;

                case onInt:
                    if (char.IsDigit(c))
                    {
                        continue;
                    }
                    if (c is not '.')
                    {
                        goto label_End;
                    }
                    innerState = floatOnDot;
                    continue;

                case hexOnX:
                    if (!char.IsAsciiHexDigit(c))
                    {
                        goto label_End;
                    }
                    innerState = hexAfterX;
                    continue;

                case hexAfterX:
                    if (!char.IsAsciiHexDigit(c))
                    {
                        goto label_End;
                    }
                    continue;

                case floatOnDot:
                    if (c is '.')
                    {
                        --pos;
                        goto label_End;
                    }
                    if (!char.IsDigit(c))
                    {
                        goto label_End;
                    }
                    innerState = floatAfterDot;
                    continue;

                case floatAfterDot:
                    if (!char.IsDigit(c))
                    {
                        goto label_End;
                    }
                    continue;
            }
        }
        label_End:
        return new NumericToken(start, pos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UnitType CheckLiteralUnit(StringView target)
        => target switch
        {
            "hp" => UnitType.HP,
            "kg" => UnitType.Kg,
            "kW" => UnitType.KW,
            "hpI" => UnitType.HpI,
            "hpM" => UnitType.HpM,
            "mph" => UnitType.MPH,
            "ton" => UnitType.Ton,
            "tons" => UnitType.Tons,
            _ => UnitType.None
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (KeywordType type, KeywordKind kind) CheckKeyword(StringView needle)
        => needle switch
        {
            "if" => (KeywordType.If, KeywordKind.BlockDefining),
            "grf" => (KeywordType.Grf, KeywordKind.BlockDefining),
            "var" => (KeywordType.Var, KeywordKind.ExpressionUsable),
            "else" => (KeywordType.Else, KeywordKind.BlockDefining),
            "item" => (KeywordType.Item, KeywordKind.BlockDefining),
            "sort" => (KeywordType.Sort, KeywordKind.FunctionBlockDefining),
            "error" => (KeywordType.Error, KeywordKind.FunctionBlockDefining),
            "param" => (KeywordType.Param, KeywordKind.ExpressionUsable),
            "while" => (KeywordType.While, KeywordKind.BlockDefining),
            "return" => (KeywordType.Return, KeywordKind.ReturnKeyword),
            "string" => (KeywordType.String, KeywordKind.ExpressionUsable),
            "switch" => (KeywordType.Switch, KeywordKind.BlockDefining),
            "produce" => (KeywordType.Produce, KeywordKind.BlockDefining),
            "replace" => (KeywordType.Replace, KeywordKind.BlockDefining),
            "basecost" => (KeywordType.BaseCost, KeywordKind.BlockDefining),
            "graphics" => (KeywordType.Graphics, KeywordKind.BlockDefining),
            "property" => (KeywordType.Property, KeywordKind.BlockDefining),
            "snowline" => (KeywordType.SnowLine, KeywordKind.BlockDefining),
            "template" => (KeywordType.Template, KeywordKind.BlockDefining),
            "spriteset" => (KeywordType.SpriteSet, KeywordKind.BlockDefining),
            "cargotable" => (KeywordType.CargoTable, KeywordKind.BlockDefining),
            "deactivate" => (KeywordType.Deactivate, KeywordKind.FunctionBlockDefining),
            "font_glyph" => (KeywordType.FontGlyph, KeywordKind.BlockDefining),
            "replacenew" => (KeywordType.ReplaceNew, KeywordKind.BlockDefining),
            "tilelayout" => (KeywordType.TileLayout, KeywordKind.BlockDefining),
            "town_names" => (KeywordType.TownNames, KeywordKind.BlockDefining),
            "spritegroup" => (KeywordType.SpriteGroup, KeywordKind.BlockDefining),
            "disable_item" => (KeywordType.DisableItem, KeywordKind.FunctionBlockDefining),
            "spritelayout" => (KeywordType.SpriteLayout, KeywordKind.BlockDefining),
            "base_graphics" => (KeywordType.BaseGraphics, KeywordKind.BlockDefining),
            "railtypetable" => (KeywordType.RailTypeTable, KeywordKind.BlockDefining),
            "random_switch" => (KeywordType.RandomSwitch, KeywordKind.BlockDefining),
            "roadtypetable" => (KeywordType.RoadTypeTable, KeywordKind.BlockDefining),
            "tramtypetable" => (KeywordType.TramTypeTable, KeywordKind.BlockDefining),
            "engine_override" => (KeywordType.EngineOverride, KeywordKind.FunctionBlockDefining),
            "livery_override" => (KeywordType.LiveryOverride, KeywordKind.BlockDefining),
            "recolour_sprite" => (KeywordType.RecolourSprite, KeywordKind.BlockDefining),
            "alternative_sprites" => (KeywordType.AlternativeSprites, KeywordKind.BlockDefining),
            _ => (KeywordType.None, KeywordKind.None)
        };
}