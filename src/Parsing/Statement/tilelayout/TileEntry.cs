using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal readonly struct TileEntry
{
    private readonly NumericToken? _x;
    private readonly BinaryOpToken? _comma;
    private readonly NumericToken? _y;
    private readonly ColonToken? _colon;
    private readonly ExpressionAST? _value;
    private readonly SemicolonToken? _semicolon;

    public TileEntry(ParsingState state, NumericToken? x, BinaryOpToken? comma = null)
    {
        _x = x;
        _comma = comma;
        var token = state.nextToken;
        while (_comma is null && token is not null)
        {
            switch (token)
            {
                case BinaryOpToken { Type: OperatorType.Comma } foundComma:
                    _comma = foundComma;
                    goto label_ParsingY;

                case BracketToken { Bracket: '}' }:
                    return;

                case ColonToken colon:
                    _colon = colon;
                    state.Increment();
                    goto label_ParsingValue;

                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                    return;

                default:
                    state.AddUnexpected(token);
                    token = state.nextToken;
                    break;
            }
        }
        label_ParsingY:
        while (token is not null)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' }:
                    return;

                case ColonToken colon:
                    _colon = colon;
                    state.Increment();
                    goto label_ParsingValue;

                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                    return;

                case NumericToken y:
                    _y = y;
                    token = state.nextToken;
                    goto label_ParsingColon;

                default:
                    state.AddUnexpected(token);
                    token = state.nextToken;
                    break;
            }
        }
        label_ParsingColon:
        while (token is not null)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' }:
                    return;

                case ColonToken colon:
                    _colon = colon;
                    state.Increment();
                    goto label_ParsingValue;

                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                    return;

                default:
                    state.AddUnexpected(token);
                    token = state.nextToken;
                    break;
            }
        }
        label_ParsingValue:
        _value = ExpressionAST.TryParse(state);
        _semicolon = state.ExpectSemicolonAfterExpression();
    }
}