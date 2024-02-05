using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal sealed partial class RecolourSprite : BaseStatementWithBlock
{
    private IReadOnlyList<Line>? _content;

    public RecolourSprite(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        List<Line> content = new();
        for (var token = state.currentToken; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                    _content = content.ToMaybeList();
                    return;

                case BracketToken { Bracket: '}' } closingBracket:
                    ClosingBracket = closingBracket;
                    state.Increment();
                    _content = content.ToMaybeList();
                    return;

                case RangeToken:
                case KeywordToken { Kind: KeywordKind.ExpressionUsable }:
                case BracketToken { Bracket: not '{' }:
                case UnaryOpToken:
                case BinaryOpToken:
                case TernaryOpToken:
                case BaseValueToken:
                    content.Add(new Line(state));
                    break;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        _content = content.ToMaybeList();
    }
}