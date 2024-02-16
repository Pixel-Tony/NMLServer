using System.Runtime.CompilerServices;

namespace NMLServer.Lexing;

internal static class Lexer
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<Token> Process(string input, in List<int> lineLengths)
        => Process(input.AsSpan(), in lineLengths).tokens;

    // TODO: add parameters for start and max symbol positions
    private static (List<Token> tokens, List<int> lineLengths) Process(ReadOnlySpan<char> input,
        in List<int> lineLengths)
    {
        LexData data = new(content: input);
        List<Token> tokens = new();
        lineLengths.Clear();
        int lineStart = 0;
        while (data.isInBounds)
        {
            char c = data.currentChar;
            if (c is '\n')
            {
                lineLengths.Add(++data.Pos - lineStart);
                lineStart = data.Pos;
                continue;
            }
            if (char.IsWhiteSpace(c))
            {
                ++data.Pos;
                continue;
            }
            if (char.IsLetter(c))
            {
                tokens.Add(ParseIdentifier(ref data));
                continue;
            }
            tokens.Add(c switch
            {
                '_' => ParseIdentifier(ref data),
                >= '0' and <= '9' => ParseNumber(c, ref data),
                ';' => new SemicolonToken(data.Pos++),
                ':' => new ColonToken(data.Pos++),
                '=' => ParseOnEqualsSign(ref data),
                '.' => ParseOnDot(ref data),
                '?' => new TernaryOpToken(data.Pos++),
                '/' => ParseFromSlash(lineLengths, ref lineStart, ref data),
                '[' or ']' or '(' or ')' or '{' or '}' => new BracketToken(data.Pos++, c),
                '\'' or '"' => ParseLiteralString(c, ref data),
                '#' => ParseHashtagComment(ref data),
                _ => TryParseOperator(c, Grammar.GetOperatorType(c), ref data)
            });
        }
        lineLengths.Add(data.Pos - lineStart);
        return (tokens, lineLengths);
    }

    private static Token ParseOnDot(ref LexData data)
    {
        if (data.isAtLastChar)
        {
            return new UnknownToken(data.Pos++);
        }
        ++data.Pos;
        return data.currentChar is '.'
            ? new RangeToken(data.Pos++ - 1)
            : new UnknownToken(data.Pos - 1);
    }

    private static Token ParseOnEqualsSign(ref LexData data)
    {
        if (data.isAtLastChar || data.nextChar is not '=')
        {
            return new AssignmentToken(data.Pos++);
        }
        var result = new BinaryOpToken(data.Pos, ++data.Pos, OperatorType.Eq);
        ++data.Pos;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Token TryParseOperator(char c, OperatorType type, ref LexData data)
    {
        if (type is OperatorType.None)
        {
            return new UnknownToken(data.Pos++);
        }
        int start = data.Pos++;
        if (data.isAtLastChar)
        {
            return c switch
            {
                '=' => new UnknownToken(start),
                '!' or '~' => new UnaryOpToken(start, c),
                _ => new BinaryOpToken(start, data.Pos, type)
            };
        }
        ReadOnlySpan<char> needle = stackalloc char[2] { c, data.currentChar };
        var opTokenType = Grammar.GetOperatorType(needle);
        if (opTokenType is OperatorType.None)
        {
            return c switch
            {
                '=' => new UnknownToken(start),
                '!' or '~' => new UnaryOpToken(start, c),
                _ => new BinaryOpToken(start, data.Pos, type)
            };
        }
        if (opTokenType is not OperatorType.ShiftRight || data.isAtLastChar)
        {
            return new BinaryOpToken(start, ++data.Pos, opTokenType);
        }
        ++data.Pos;
        return data.currentChar is '>'
            ? new BinaryOpToken(start, ++data.Pos, OperatorType.ShiftRightFunky)
            : new BinaryOpToken(start, data.Pos, opTokenType);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static CommentToken ParseHashtagComment(ref LexData data)
    {
        int start = data.Pos++;
        while (data.isInBounds)
        {
            char c = data.currentChar;
            if (c == '\n')
            {
                break;
            }
            data.Pos++;
        }
        return new CommentToken(start, data.Pos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Token ParseFromSlash(ICollection<int> lineLengths, ref int lineStart, ref LexData data)
    {
        int start = data.Pos++;
        if (data.isAtLastChar)
        {
            return new BinaryOpToken(start, data.Pos, OperatorType.Divide);
        }
        switch (data.currentChar)
        {
            case '*':
                ++data.Pos;
                while (data.isInBounds && data.Pos - start < 3)
                {
                    if (data.currentChar is '\n')
                    {
                        lineLengths.Add(++data.Pos - lineStart);
                        lineStart = data.Pos;
                        continue;
                    }
                    ++data.Pos;
                }
                char prev = data.Content[data.Pos - 1];
                while (data.isInBounds)
                {
                    var current = data.currentChar;
                    if (current is '\n')
                    {
                        prev = '\n';
                        lineLengths.Add(data.Pos - lineStart);
                        lineStart = ++data.Pos;
                        continue;
                    }
                    if (prev is '*' && current is '/')
                    {
                        break;
                    }
                    prev = current;
                    ++data.Pos;
                }
                return new CommentToken(start, ++data.Pos);

            case '/':
                ++data.Pos;
                while (data.isInBounds)
                {
                    if (data.currentChar is '\n')
                    {
                        break;
                    }
                    ++data.Pos;
                }
                return new CommentToken(start, data.Pos);

            default:
                return new BinaryOpToken(start, data.Pos, OperatorType.Divide);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Token ParseLiteralString(char openingQuote, ref LexData data)
    {
        int start = data.Pos++;
        while (data.isInBounds)
        {
            char c = data.currentChar;
            ++data.Pos;
            if (c == openingQuote || c is '\n')
            {
                break;
            }
        }
        return new StringToken(start, data.Pos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Token ParseIdentifier(ref LexData data)
    {
        int start = data.Pos++;
        char current = '\0';
        for (; data.isInBounds; ++data.Pos)
        {
            current = data.currentChar;
            if (!IsValidIdentifierCharacter(current))
            {
                break;
            }
        }
        var value = data.Content[start..data.Pos];
        switch (current)
        {
            case '/' when data.MaxPos - data.Pos == 1:
            case '/' when data.MaxPos - data.Pos > 1 && !IsValidIdentifierCharacter(data.Content[data.Pos + 2]):
                switch (data.Pos - start)
                {
                    case 1 when value is "m" && data.nextChar is 's':
                        data.Pos += 2;
                        return new UnitToken(start, UnitType.MPS);

                    case 2 when value is "km" && data.nextChar is 'h':
                        data.Pos += 2;
                        return new UnitToken(start, UnitType.KMPH);
                }
                break;

            case '%' when value is "snow":
                ++data.Pos;
                return new UnitToken(start, UnitType.Snow);
        }

        for (var unitType = CheckLiteralUnit(value); unitType is not UnitType.None;)
        {
            return new UnitToken(start, unitType);
        }
        for (var (keywordType, kind) = CheckKeyword(value); keywordType is not KeywordType.None;)
        {
            return new KeywordToken(start, data.Pos, keywordType, kind);
        }
        return new IdentifierToken(start, data.Pos, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Token ParseNumber(char c, ref LexData data)
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
        int start = data.Pos++;
        for (; data.isInBounds; ++data.Pos)
        {
            c = data.currentChar;
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
                        --data.Pos;
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
        return new NumericToken(start, data.Pos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidIdentifierCharacter(char c) => c is '_' || char.IsLetterOrDigit(c);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UnitType CheckLiteralUnit(ReadOnlySpan<char> target)
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
    private static (KeywordType type, KeywordKind kind) CheckKeyword(ReadOnlySpan<char> needle)
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

    private ref struct LexData
    {
        public readonly int MaxPos;
        public int Pos;
        public readonly ReadOnlySpan<char> Content;

        public char currentChar => Content[Pos];

        public char nextChar => Content[Pos + 1];

        public bool isAtLastChar => Pos == MaxPos;

        public bool isInBounds => Pos <= MaxPos;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LexData(ReadOnlySpan<char> content)
        {
            MaxPos = content.Length - 1;
            Pos = 0;
            Content = content;
        }
    }
}