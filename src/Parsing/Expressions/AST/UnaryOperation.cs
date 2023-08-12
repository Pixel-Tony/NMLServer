using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal class UnaryOperation : ExpressionAST
{
    private readonly UnaryOpToken _operation;
    public ExpressionAST? Expression;

    public UnaryOperation(ExpressionAST? parent, UnaryOpToken operation) : base(parent)
    {
        _operation = operation;
    }

    public override string ToString() => $"({_operation.Sign}{Expression?.ToString() ?? "."})";
}