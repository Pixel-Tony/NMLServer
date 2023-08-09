// using NMLServer.Lexing.Tokens;
//
// namespace NMLServer.Parsing.Expression;
//
// internal class FunctionCall : ExpressionAST
// {
//     public LiteralToken? Function;
//     public ExpressionAST? Arguments;
//     public (BracketToken?, BracketToken?) Parens;
//
//     public FunctionCall(ExpressionAST? parent) : base(parent) {}
//     
//     public override string ToString() => $"(Func: {Function.value}({Arguments}))";
// }