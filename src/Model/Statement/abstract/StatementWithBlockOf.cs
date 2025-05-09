using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal abstract class StatementWithBlockOf<T> : StatementWithBlock where T : IAllowsParseInsideBlock<T>
{
    protected readonly List<T>? Contents;

    protected sealed override int middleEnd => Contents?[^1].end ?? 0;

    protected StatementWithBlockOf(ref ParsingState state, KeywordToken keyword) : base(ref state, keyword)
    {
        if (ClosingBracket is null)
        {
            Contents = T.ParseSomeInBlock(ref state, ref ClosingBracket);
        }
    }
}