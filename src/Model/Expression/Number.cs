using NMLServer.Model.Lexis;

namespace NMLServer.Model.Expression;

internal sealed class Number(ExpressionAST? parent, NumericToken token) : BaseValueNode<NumericToken>(parent, token);