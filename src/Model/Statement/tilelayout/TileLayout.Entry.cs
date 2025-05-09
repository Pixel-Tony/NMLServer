using NMLServer.Model.Lexis;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal sealed partial class TileLayout
{
    internal readonly struct Entry : IHasEnd
    {
        private readonly NumericToken? _x;
        private readonly BinaryOpToken? _comma;
        private readonly NumericToken? _y;
        private readonly ColonToken? _colon;
        private readonly ExpressionAST? _value;
        private readonly SemicolonToken? _semicolon;

        public int end => _semicolon?.end ?? (_value?.end ?? (_colon?.end ?? (_y?.end ?? (_comma?.end ?? _x!.end))));

        public Entry(ref ParsingState state, NumericToken? x, BinaryOpToken? comma = null)
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

                    case ColonToken colon:
                        _colon = colon;
                        state.IncrementSkippingComments();
                        goto label_ParsingValue;

                    case BracketToken { Bracket: '}' }:
                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.CallDefining }:
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
                    case NumericToken y:
                        _y = y;
                        token = state.nextToken;
                        goto label_ParsingColon;

                    case ColonToken colon:
                        _colon = colon;
                        state.IncrementSkippingComments();
                        goto label_ParsingValue;

                    case BracketToken { Bracket: '}' }:
                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.CallDefining }:
                        return;

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
                    case ColonToken colon:
                        _colon = colon;
                        state.IncrementSkippingComments();
                        goto label_ParsingValue;

                    case BracketToken { Bracket: '}' }:
                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.CallDefining }:
                        return;

                    default:
                        state.AddUnexpected(token);
                        token = state.nextToken;
                        break;
                }
            }
            label_ParsingValue:
            _value = ExpressionAST.TryParse(ref state);
            _semicolon = state.ExpectSemicolon();
        }
    }
}