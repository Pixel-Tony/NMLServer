namespace NMLServer.Parsing.Statement;

internal abstract class BaseDeclaration : BaseStatementAST
{
    protected BaseDeclaration(NMLFileRoot parent) : base(parent)
    { }
}