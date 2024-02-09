using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal abstract class BaseBlockWithChildren : BaseStatementWithBlock
{
    protected readonly NMLFile Children;

    protected BaseBlockWithChildren(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        Children = new NMLFile(state, out ClosingBracket, true);
    }
}