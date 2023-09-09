using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement.Models;

internal class FunctionStatement : BaseStatement
{
    public StatementHeading Heading;
    public SemicolonToken? Semicolon;

    public FunctionStatement()
    { }

    public FunctionStatement(KeywordToken keyword)
    {
        Heading = new StatementHeading(keyword);
    }

    public FunctionStatement(StatementHeading heading)
    {
        Heading = heading;
    }
}