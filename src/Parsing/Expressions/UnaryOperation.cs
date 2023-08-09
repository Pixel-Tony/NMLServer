using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

// internal class UnaryOperation : ExpressionAST
// {
//     public UnaryOpToken Operation;
//     public ExpressionAST? Expression;
//
//     public UnaryOperation(ExpressionAST? parent) : base(parent) { }
//     
//     public override string ToString()
//     {
//         return $"(Unary {(Operation.IsLogical ? '!' : '~')}: {Expression})";
//     }
// }