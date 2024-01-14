using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statements.Models;

internal class FunctionStatement : StatementWithParameters
{
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