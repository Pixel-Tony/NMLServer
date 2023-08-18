namespace NMLServer.Parsing.Statement;

internal abstract class BaseStatementAST
{
    public BaseStatementAST? Parent;

    protected BaseStatementAST(BaseStatementAST? parent) => Parent = parent;
}