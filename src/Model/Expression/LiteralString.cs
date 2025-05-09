using NMLServer.Model.Lexis;

namespace NMLServer.Model.Expression;

internal sealed class LiteralString(ExpressionAST? parent, StringToken token)
    : BaseValueNode<StringToken>(parent, token);