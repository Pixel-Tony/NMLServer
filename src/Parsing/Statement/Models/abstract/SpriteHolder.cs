using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statements.Models;

internal abstract class SpriteHolder : StatementWithParametersAndBody
{
    public ExpressionAST[]? Content;
}