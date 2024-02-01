using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal class RecolourSprite : BaseStatementWithBlock
{
    private RecolourLine[]? _content;

    public RecolourSprite(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        List<RecolourLine> content = new();
        for (var token = state.currentToken; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    _content = content.ToArrayOrNull();
                    return;

                case BracketToken { Bracket: '}' } closingBracket:
                    ClosingBracket = closingBracket;
                    state.Increment();
                    _content = content.ToArrayOrNull();
                    return;

                case RangeToken:
                case KeywordToken { Kind: KeywordKind.ExpressionUsable }:
                case BracketToken { Bracket: not '{' }:
                case UnaryOpToken:
                case BinaryOpToken:
                case TernaryOpToken:
                case BaseValueToken:
                    content.Add(new RecolourLine(state));
                    break;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        _content = content.ToArrayOrNull();
    }

    private readonly record struct RecolourLine
    {
        private readonly ExpressionAST? _leftLeft;
        private readonly RangeToken? _leftRange;
        private readonly ExpressionAST? _leftRight;
        private readonly ColonToken? _colon;
        private readonly ExpressionAST? _rightLeft;
        private readonly RangeToken? _rightRange;
        private readonly ExpressionAST? _rightRight;
        private readonly SemicolonToken? _semicolon;

        public RecolourLine(ParsingState state)
        {
            ExpressionOrRange(state, ref _leftLeft, ref _leftRange, ref _leftRight);
            for (var token = state.currentToken; token is not null; token = state.nextToken)
            {
                switch (token)
                {
                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    case BracketToken { Bracket: '}' }:
                        return;

                    case ColonToken colonToken:
                        _colon = colonToken;
                        state.Increment();
                        goto label_Right;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
            return;

            label_Right:
            ExpressionOrRange(state, ref _rightLeft, ref _rightRange, ref _rightRight, false);
            for (var token = state.currentToken; token is not null; token = state.nextToken)
            {
                switch (token)
                {
                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    case BracketToken { Bracket: '}' }:
                        return;

                    case SemicolonToken semicolonToken:
                        _semicolon = semicolonToken;
                        state.Increment();
                        return;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
        }

        private static void ExpressionOrRange(ParsingState state, ref ExpressionAST? left, ref RangeToken? range,
            ref ExpressionAST? right, bool stopAtColon = true)
        {
            for (var token = state.currentToken; token is not null; token = state.nextToken)
            {
                switch (token)
                {
                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    case BracketToken { Bracket: '}' }:
                        return;

                    case KeywordToken { Kind: KeywordKind.ExpressionUsable }:
                    case BracketToken { Bracket: not '{' }:
                    case UnaryOpToken:
                    case BinaryOpToken:
                    case TernaryOpToken:
                    case BaseValueToken:
                        left = ExpressionAST.TryParse(state);
                        goto label_Range;

                    case RangeToken rangeToken:
                        range = rangeToken;
                        state.Increment();
                        goto label_Right;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
            return;

            label_Range:
            for (var token = state.currentToken; token is not null; token = state.nextToken)
            {
                switch (token)
                {
                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    case BracketToken { Bracket: '}' }:
                    case ColonToken when stopAtColon:
                    case SemicolonToken when !stopAtColon:
                        return;

                    case RangeToken rangeToken:
                        range = rangeToken;
                        state.Increment();
                        goto label_Right;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
            return;

            label_Right:
            for (var token = state.currentToken; token is not null; token = state.nextToken)
            {
                switch (token)
                {
                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    case BracketToken { Bracket: '}' }:
                    case ColonToken when stopAtColon:
                    case SemicolonToken when !stopAtColon:
                        return;

                    case KeywordToken { Kind: KeywordKind.ExpressionUsable }:
                    case BracketToken { Bracket: not '{' }:
                    case UnaryOpToken:
                    case BinaryOpToken:
                    case TernaryOpToken:
                    case BaseValueToken:
                        right = ExpressionAST.TryParse(state);
                        return;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
        }
    }
}