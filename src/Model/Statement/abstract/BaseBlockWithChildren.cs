using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal abstract class BaseBlockWithChildren : BaseStatementWithBlockOf<BaseStatement>
{
    protected BaseBlockWithChildren(ParsingState state, KeywordToken keyword) : base(state, keyword)
    { }
}