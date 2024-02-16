using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal abstract class BaseStatementWithBlockOf<T> : BaseStatementWithBlock where T : IAllowsParseInsideBlock<T>
{
    protected readonly List<T>? Contents;

    protected sealed override int middleEnd => Contents?[^1].end ?? 0;

    protected BaseStatementWithBlockOf(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is null)
        {
            Contents = T.ParseSomeInBlock(state, ref ClosingBracket);
        }
    }
}