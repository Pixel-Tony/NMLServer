using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class ParentedExpression : ExpressionAST
{
    public readonly BracketToken? OpeningBracket;
    public ExpressionAST? Expression;
    public BracketToken? ClosingBracket;

    public override int start => OpeningBracket?.Start ?? Expression?.start ?? ClosingBracket!.Start;

    public override int end
        => ClosingBracket is not null
            ? ClosingBracket.Start + 1
            : Expression?.end ?? OpeningBracket!.Start + 1;

    public ParentedExpression(
        ExpressionAST? parent = null,
        BracketToken? openingBracket = null,
        ExpressionAST? expression = null,
        BracketToken? closingBracket = null
    ) : base(parent)
    {
        OpeningBracket = openingBracket;
        Expression = expression;
        ClosingBracket = closingBracket;
    }

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