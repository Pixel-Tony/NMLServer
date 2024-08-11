using NMLServer.Lexing;

namespace NMLServer.Model.Expression;

internal sealed class Number(ExpressionAST? parent, NumericToken token) : BaseValueNode<NumericToken>(parent, token);