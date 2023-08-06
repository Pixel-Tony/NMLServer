using System.Runtime.CompilerServices;
using System.Text;
using NMLServer.Lexing.Tokens;
using NMLServer.Parsing;

namespace NMLServer.Lexing;

internal class Lexer
{
    private static readonly HashSet<char> _opStarts = new(Grammar.Operators.Select(o => o[0]));
    
    private static readonly HashSet<char> _brackets = new("[]{}()");

    private readonly string _input;
    private readonly int _maxPos;
    private int _pos;

    private char charPointedAt => _input[_pos];

    public Lexer(string inputString)
    {
        _input = inputString;
        _maxPos = inputString.Length - 1;
    }

    public IEnumerable<Token> Tokenize()
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
                    yield return Consume(c, new SemicolonToken());
                    continue;
                case ':':
                    yield return Consume(c, new ColonToken());
                    continue;
                case '?':
                    yield return Consume(c, new TernaryOpToken());
                    continue;
            }

            if (IsValidIdStartCharacter(c))
            {
                yield return ParseIdentifier(c);
                continue;
            }

            if (char.IsDigit(c))
            {
                yield return ParseNumber(c);
                continue;
            }

            if (_brackets.Contains(c))
            {
                yield return ParseBracket(c);
                continue;
            }

            // Checked separately to consume (//...) and (/*...*/) comments,
            // also parses (/) division operator
            if (c == '/')
            {
                yield return ParseFromSlash();
                continue;
            }

            if (_opStarts.Contains(c))
            {
                yield return ParseOperator(c);
                continue;
            }

            yield return c switch
            {
                '\'' or '"' => ParseLiteralString(c),
                '#' => ParseHashtagComment(),
                '/' => ParseFromSlash(),
                _ => Consume(c, new FailedToken(c))
            };
        }
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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private Token ParseOperator(char c)
    {
        // ? and : are handled earlier
        
        string opChar = c.ToString();
        if (_pos == _maxPos)
        {
            Token token = DecideOperatorType(c, opChar);
            return Consume(c, token);
        }

        _pos++;
        var withNextChar = opChar + charPointedAt;
        if (!Grammar.Operators.Contains(withNextChar))
        {
            return DecideOperatorType(c, opChar);
        }
        if (withNextChar != ">>" || _pos >= _maxPos)
        {
            return Consume(charPointedAt, new BinaryOpToken(withNextChar));
        }
        _pos++;
        return charPointedAt == '>' 
            ? Consume('>', new BinaryOpToken(">>>")) 
            : new BinaryOpToken(withNextChar);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Token Consume(char consumed, Token t)
    {
        _pos++;
        return t;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private Token ParseHashtagComment()
    {
        StringBuilder builer = new StringBuilder().Append('#');

        _pos++;
        while (_pos <= _maxPos)
        {
            char c = charPointedAt;
            _pos++;
            builer.Append(c);
            if (c == '\n')
            {
                break;
            }
        }

        return new LiteralToken(builer.ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private Token ParseFromSlash()
    {
        LiteralToken token = ConsumeIntoNew('/');
        char c = charPointedAt;
        switch (c)
        {
            case '*':
            {
                _pos++;
                while (_pos <= _maxPos && !(token.value.EndsWith("*/") && token.value.Length > 3))
                {
                    ConsumeInto(token, charPointedAt);
                }

                return token;
            }
            case '/':
            {
                _pos++;
                while (_pos <= _maxPos)
                {
                    c = charPointedAt;
                    ConsumeInto(token, c);
                    if (c == '\n')
                    {
                        return token;
                    }
                }

                return token;
            }
            default:
                return new BinaryOpToken("/");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private Token ParseLiteralString(char c)
    {
        char openingQuote = c;
        LiteralToken token = new();
        ConsumeInto(token, openingQuote);
        while (_pos <= _maxPos)
        {
            c = charPointedAt;
            ConsumeInto(token, c);
            if (c == openingQuote)
            {
                break;
            }
        }

        return token;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private Token ParseBracket(char bracket)
    {
        _pos++;
        return new BracketToken(bracket);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private Token ParseIdentifier(char c)
    {
        StringBuilder identifierToken = new();
        ConsumeIntoBuilder(identifierToken, c);

        _pos++;
        while (_pos <= _maxPos)
        {
            c = charPointedAt;
            if (!IsValidIdentifierCharacter(c))
            {
                break;
            }

            ConsumeIntoBuilder(identifierToken, c);
        }
        var value = identifierToken.ToString();
        if (Grammar.Keywords.Contains(value))
        {
            return new KeywordToken(value);
        }
        if (Grammar.Features.Contains(value))
        {
            return new FeatureToken(value);
        }
        return new LiteralToken(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private LiteralToken ConsumeIntoNew(char c)
    {
        _pos++;
        return new LiteralToken(c);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ConsumeInto(BaseRecordingToken t, char c)
    {
        _pos++;
        t.Add(c);
    }

    private void ConsumeIntoBuilder(StringBuilder tokenBuilder, char c)
    {
        _pos++;
        tokenBuilder.Append(c);
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private Token ParseNumber(char c)
    {
        StringBuilder numberBuilder = new(16);
        var state = c == '0' ? NumberLexState.StartingZero : NumberLexState.Int;

        ConsumeIntoBuilder(numberBuilder, c);
        while (_pos <= _maxPos)
        {
            c = charPointedAt;
            switch (state)
            {
                case NumberLexState.StartingZero:
                    if (c == 'x' || c == 'X')
                    {
                        ConsumeIntoBuilder(numberBuilder, c);
                        state = NumberLexState.HexOnX;
                        continue;
                    }

                    if (!char.IsDigit(c))
                    {
                        break;
                    }

                    ConsumeIntoBuilder(numberBuilder, c);
                    state = NumberLexState.Int;
                    continue;

                case NumberLexState.Int:
                    if (char.IsDigit(c))
                    {
                        ConsumeIntoBuilder(numberBuilder, c);
                        continue;
                    }

                    if (c != '.')
                    {
                        break;
                    }

                    ConsumeIntoBuilder(numberBuilder, c);
                    state = NumberLexState.FloatOnDot;
                    continue;

                case NumberLexState.HexOnX:
                    if (!char.IsAsciiHexDigit(c))
                    {
                        break;
                    }

                    ConsumeIntoBuilder(numberBuilder, c);
                    state = NumberLexState.HexAfterX;
                    continue;

                case NumberLexState.HexAfterX:
                    if (!char.IsAsciiHexDigit(c))
                    {
                        break;
                    }

                    ConsumeIntoBuilder(numberBuilder, c);
                    continue;

                case NumberLexState.FloatOnDot:
                    if (!char.IsDigit(c))
                    {
                        break;
                    }

                    ConsumeIntoBuilder(numberBuilder, c);
                    state = NumberLexState.FloatAfterDot;
                    continue;

                case NumberLexState.FloatAfterDot:
                    if (!char.IsDigit(c))
                    {
                        break;
                    }

                    ConsumeIntoBuilder(numberBuilder, c);
                    continue;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            break;
        }
        
        return new NumericToken(numberBuilder.ToString());
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

