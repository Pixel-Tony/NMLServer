// using NMLServer.Lexing.Tokens;
//
// namespace NMLServer.Parsing.Expression;
//
// internal class ArrayExpression : ExpressionAST
// {
//     public BracketToken? OpeningBracket;
//     public ExpressionAST? Expression;
//     public BracketToken? ClosingBracket;
//
//     public ArrayExpression(ExpressionAST parent) : base(parent) 
//     { }
//
//     public override ExpressionAST ReplaceWith<T>(ExpressionAST target)
//     {
//         if (Expression != target)
//         {
//             throw new Exception();
//         }
//         Expression = typeof(T).GetConstructor(new[] { typeof(T) })!.Invoke(new object[] { this }) as ExpressionAST;
//         return Expression!;
//     }
//
//     public override string ToString() => $"Array [{Expression}]";
// }