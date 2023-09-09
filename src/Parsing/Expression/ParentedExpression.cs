using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class ParentedExpression : ExpressionAST
{
    private readonly BracketToken? _openingBracket;
    public ExpressionAST? Expression;
    public BracketToken? ClosingBracket;

    public ParentedExpression(ExpressionAST? parent, BracketToken? openingBracket) : base(parent)
        => _openingBracket = openingBracket;

    public ParentedExpression() : base(null)
    { }

    public override void Replace(ExpressionAST target, ExpressionAST value)
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
            _ => throw new Exception()
        };
    }
}