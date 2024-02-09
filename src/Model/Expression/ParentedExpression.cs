using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class ParentedExpression : ExpressionAST
{
    private readonly BracketToken? _openingBracket;
    public ExpressionAST? Expression;
    public BracketToken? ClosingBracket;

    public ParentedExpression(
        ExpressionAST? parent = null,
        BracketToken? openingBracket = null,
        ExpressionAST? expression = null,
        BracketToken? closingBracket = null
    ) : base(parent)
    {
        _openingBracket = openingBracket;
        Expression = expression;
        ClosingBracket = closingBracket;
    }

    protected override void Replace(ExpressionAST target, ExpressionAST value)
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