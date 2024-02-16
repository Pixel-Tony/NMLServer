using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal class ElseBlock : BaseStatementWithBlock
{
    private readonly IReadOnlyList<BaseStatement>? _children;
    protected override int middleEnd => _children?[^1].end ?? 0;

    public ElseBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is null && OpeningBracket is not null)
        {
            _children = ParseSomeInBlock(state, ref ClosingBracket);
        }
    }
}