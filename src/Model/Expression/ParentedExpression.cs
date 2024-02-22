using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class ParentedExpression(
    ExpressionAST? parent = null,
    BracketToken? openingBracket = null,
    ExpressionAST? expression = null,
    BracketToken? closingBracket = null)
    : ExpressionAST(parent)
{
    public readonly BracketToken? OpeningBracket = openingBracket;
    public ExpressionAST? Expression = expression;
    public BracketToken? ClosingBracket = closingBracket;

    public override int start => OpeningBracket?.start ?? Expression?.start ?? ClosingBracket!.start;

    public override int end
        => ClosingBracket is not null
            ? ClosingBracket.start + 1
            : Expression?.end ?? OpeningBracket!.start + 1;

    protected override void Replace(ExpressionAST target, FunctionCall value)
    {
        Expression = value;
    }

    public bool Matches(BracketToken closingBracket)
    {
        return closingBracket.Bracket == OpeningBracket!.Bracket switch
        {
            '(' => ')',
            '[' => ']',
            '{' => '}',
            _ => '\0'
        };
    }
}