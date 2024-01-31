using System.Runtime.CompilerServices;
using NMLServer.Lexing.Tokens;

namespace NMLServer.Lexing;

internal ref struct Lexer
{
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
            if (c is '_' || char.IsLetter(c))
            {
                tokens.Add(ParseIdentifier());
                continue;
            }
            switch (c)
            {
                case >= '0' and <= '9':
                    tokens.Add(ParseNumber(c));
                    break;

                case '=':
                    if (_pos == _maxPos || _context[_pos + 1] is not '=')
                    {
                        tokens.Add(new AssignmentToken(_pos++));
                        continue;
                    }
                    tokens.Add(new BinaryOpToken(_pos, ++_pos, OperatorType.Eq));
                    ++_pos;
                    break;

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
                    break;

                case ';':
                    tokens.Add(new SemicolonToken(_pos++));
                    break;

                case ':':
                    tokens.Add(new ColonToken(_pos++));
                    break;

                case '?':
                    tokens.Add(new TernaryOpToken(_pos++));
                    break;

                case '/':
                    tokens.Add(ParseFromSlash(lineLengths, ref lineStart));
                    break;

                case '[' or ']' or '(' or ')' or '{' or '}':
                    tokens.Add(new BracketToken(_pos++, c));
                    break;

                case '\'' or '"':
                    tokens.Add(ParseLiteralString(c));
                    break;

                case '#':
                    tokens.Add(ParseHashtagComment());
                    break;

                default:
                    var type = Grammar.GetOperatorType(c);
                    tokens.Add(
                        type is not OperatorType.None
                            ? ParseOperator(c, type)
                            : new FailedToken(_pos++)
                    );
                    break;
            }
        }
        lineLengths.Add(_pos - lineStart);
        return (tokens, lineLengths);
    }

    private Token ParseOperator(char c, OperatorType type)
    {
        int start = _pos++;
        if (_pos == _maxPos)
        {
            return c switch
            {
                '=' => new FailedToken(start),
                '!' or '~' => new UnaryOpToken(start, c),
                _ => new BinaryOpToken(start, _pos, type)
            };
        }
        ReadOnlySpan<char> needle = stackalloc char[2] { c, GetCurrentChar() };

        for (var opTokenType = Grammar.GetOperatorType(needle);
             opTokenType is not OperatorType.None;)
        {
            if (opTokenType is not OperatorType.ShiftRight || _pos >= _maxPos)
            {
                return new BinaryOpToken(start, ++_pos, opTokenType);
            }
            ++_pos;
            return GetCurrentChar() is '>'
                ? new BinaryOpToken(start, _pos++, OperatorType.ShiftRightFunky)
                : new BinaryOpToken(start, _pos, opTokenType);
        }
        return c switch
        {
            '=' => new FailedToken(start),
            '!' or '~' => new UnaryOpToken(start, c),
            _ => new BinaryOpToken(start, _pos, type)
        };
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
            return new BinaryOpToken(start, _pos, OperatorType.Divide);
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
                return new BinaryOpToken(start, _pos, OperatorType.Divide);
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

    private Token ParseIdentifier()
    {
        int start = _pos++;
        char current = default;
        for (; _pos <= _maxPos; ++_pos)
        {
            current = GetCurrentChar();
            if (!IsValidIdentifierCharacter(current))
            {
                break;
            }
        }
        var value = _context[start.._pos];
        switch (current)
        {
            case '/' when _maxPos - _pos == 1:
            case '/' when _maxPos - _pos > 1 && !IsValidIdentifierCharacter(_context[_pos + 2]):
                switch (_pos - start)
                {
                    case 1 when value is "m" && _context[_pos + 1] is 's':
                        _pos += 2;
                        return new UnitToken(start, UnitType.MPS, 3);

                    case 2 when value is "km" && _context[_pos + 1] is 'h':
                        _pos += 2;
                        return new UnitToken(start, UnitType.KMPH, 4);
                }
                break;

            case '%' when value is "snow":
                ++_pos;
                return new UnitToken(start, UnitType.Snow, 5);
        }
        for (UnitType unitType = CheckLiteralUnit(value); unitType is not UnitType.None;)
        {
            return new UnitToken(start, unitType, value.Length);
        }
        for (var (keywordType, kind) = CheckKeyword(value); keywordType is not KeywordType.None;)
        {
            return new KeywordToken(start, _pos, keywordType, kind);
        }
        return new IdentifierToken(start, _pos);
    }

    private Token ParseNumber(char c)
    {
        var state = c is '0'
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
                    if (c is not '.')
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

    private static bool IsValidIdentifierCharacter(char c) => c is '_' || char.IsLetterOrDigit(c);

    private enum NumberLexState : byte
    {
        StartingZero,
        Int,
        HexOnX,
        HexAfterX,
        FloatOnDot,
        FloatAfterDot
    }

    private static UnitType CheckLiteralUnit(ReadOnlySpan<char> target)
        => target switch
        {
            "kg" => UnitType.Kg,
            "hp" => UnitType.HP,
            "kW" => UnitType.KW,
            "mph" => UnitType.MPH,
            "ton" => UnitType.Ton,
            "hpI" => UnitType.HpI,
            "hpM" => UnitType.HpM,
            "tons" => UnitType.Tons,
            _ => UnitType.None
        };

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