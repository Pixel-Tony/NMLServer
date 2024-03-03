using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal abstract class BaseBlockWithChildren : BaseStatementWithBlock
{
    protected readonly List<BaseStatement>? Children;

    protected sealed override int middleEnd => Children?[^1].end ?? 0;

    protected BaseBlockWithChildren(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is null && OpeningBracket is not null)
        {
            Children = ParseSomeInBlock(state, ref ClosingBracket);
        }
    }
}