using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal abstract class BaseParametrizedBlockWithChildren : BaseParametrizedStatementWithBlock
{
    protected readonly NMLFile Children;

    protected BaseParametrizedBlockWithChildren(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        Children = new NMLFile(state, ref ClosingBracket);
    }
}