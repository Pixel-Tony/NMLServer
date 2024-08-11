using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class ParentedExpression(
    ExpressionAST? parent = null,
    BracketToken? openingBracket = null,
    ExpressionAST? expression = null,
    BracketToken? closingBracket = null)
    : ExpressionAST(parent)
{
    private readonly BracketToken? _openingBracket = openingBracket;
    public ExpressionAST? Expression = expression;
    public BracketToken? ClosingBracket = closingBracket;

    public override int start => _openingBracket?.start ?? Expression?.start ?? ClosingBracket!.start;

    public override int end
        => ClosingBracket is not null
            ? ClosingBracket.start + 1
            : Expression?.end ?? _openingBracket!.start + 1;

    protected override void Replace(ExpressionAST target, FunctionCall value)
    {
        Expression = value;
    }

    public bool Matches(BracketToken closingBracket)
    {
        return closingBracket.Bracket == _openingBracket!.Bracket switch
        {
            '(' => ')',
            '[' => ']',
            '{' => '}',
            _ => '\0'
        };
    }
}