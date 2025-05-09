using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal abstract class InnerStatementNode : StatementWithBlock
{
    // TODO remove if not used, previously used only while constructing ast, can be replaced with a stack
    public readonly InnerStatementNode? Parent;
    public readonly List<StatementAST>? Children;

    protected sealed override int middleEnd => Children?[^1].end ?? 0;

    protected InnerStatementNode(InnerStatementNode? parent, ref ParsingState state, KeywordToken keyword)
        : base(ref state, keyword)
    {
        Parent = parent;
        if (OpeningBracket is not null && ClosingBracket is null)
        {
            Children = [];
        }
    }
}