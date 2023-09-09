using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class FunctionStatement : BaseStatement
{
    public StatementHeadingParseResult Heading;
    public SemicolonToken? Semicolon;

    public FunctionStatement()
    { }

    public FunctionStatement(KeywordToken keyword)
    {
        Heading = new StatementHeadingParseResult(keyword);
    }

    public FunctionStatement(StatementHeadingParseResult heading)
    {
        Heading = heading;
    }
}