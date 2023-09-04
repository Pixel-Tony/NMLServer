namespace NMLServer.Parsing.Statement;

internal sealed class NMLFileRoot : BaseStatementAST
{
    public readonly List<BaseStatementAST> Children = new();

    public NMLFileRoot() : base(null)
    { }
}