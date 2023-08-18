using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class ParentedExpression : ExpressionAST
{
    public readonly BracketToken OpeningBracket;
    public ExpressionAST? Expression;
    public BracketToken? ClosingBracket;

    public ParentedExpression(ExpressionAST? parent, BracketToken openingBracket) : base(parent)
        => OpeningBracket = openingBracket;

    public override string ToString() 
        => $"{OpeningBracket.ToString()}{Expression}{ClosingBracket?.ToString() ?? "."}";
}