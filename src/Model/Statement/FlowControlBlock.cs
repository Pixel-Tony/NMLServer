using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal class FlowControlBlock(
    InnerStatementNode? parent,
    ref ParsingState state,
    KeywordToken keyword,
    KeywordType type) : InnerStatementNode(parent, ref state, keyword)
{
    public readonly KeywordType Type = type;
}