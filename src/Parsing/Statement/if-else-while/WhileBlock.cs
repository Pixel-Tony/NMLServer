using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class WhileBlock : BaseParametrizedStatementWithBlock
{
    private readonly NMLFile _children;

    public WhileBlock(ParsingState state, KeywordToken keyword) : base(state, keyword)
    {
        if (ClosingBracket is not null)
        {
            return;
        }
        _children = new NMLFile(state, ref ClosingBracket);
    }
}