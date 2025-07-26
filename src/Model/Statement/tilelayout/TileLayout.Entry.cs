using NMLServer.Model.Lexis;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal sealed partial class TileLayout
{
    private readonly struct Entry : IHasEnd
    {
        private readonly NumericToken? _x;
        private readonly BinaryOpToken? _comma;
        private readonly NumericToken? _y;
        private readonly ColonToken? _colon;
        private readonly ExpressionAST? _value;
        private readonly SemicolonToken? _semicolon;

        public int End => _semicolon?.End ?? _value?.End ?? _colon?.End ?? _y?.End ?? _comma?.End ?? _x!.End;

        public Entry(ref ParsingState state, NumericToken? x, BinaryOpToken? comma = null)
        {
            _x = x;
            _comma = comma;
            while (state.NextToken is { } token)
            {
                switch (token)
                {
                    case BinaryOpToken { Type: OperatorType.Comma } foundComma
                            when (_comma is null) & (_y is null):
                        _comma = foundComma;
                        break;

                    case ColonToken colon when _colon is null:
                        _colon = colon;
                        state.IncrementSkippingComments();
                        _value = ExpressionAST.TryParse(ref state);
                        _semicolon = state.ExpectSemicolon();
                        return;

                    case NumericToken y when (_comma is not null) & (_y is null):
                        _y = y;
                        break;

                    case BracketToken { Bracket: '}' }:
                    case KeywordToken { Kind: KeywordKind.BlockDefining }:
                        return;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
            return;
        }
    }
}