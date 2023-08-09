// using NMLServer.Lexing.Tokens;
//
// namespace NMLServer.Parsing.Expression;
//
// internal class ParentedExpression : ExpressionAST
// {
//     public ExpressionAST? Inner;
//     public (BracketToken? opening, BracketToken? closing) Parentheses;
//
//     public ParentedExpression(ExpressionAST? parent) : base(parent)
//     { }
//
//     public override string ToString() => $"(Parens: {Inner})";
// }