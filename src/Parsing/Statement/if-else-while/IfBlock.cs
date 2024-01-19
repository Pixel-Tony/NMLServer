using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class IfBlock : BaseParametrizedStatementWithBlock
{
    private readonly NMLFile _children;

    public IfBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        _children = new NMLFile(state, ref ClosingBracket);
    }
}