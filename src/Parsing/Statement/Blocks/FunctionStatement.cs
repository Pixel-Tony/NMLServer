using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class FunctionStatement : BaseStatementAST
{
    public StatementHeadingParseResult Heading;
    public SemicolonToken? Semicolon;

    public FunctionStatement(BaseStatementAST? parent) : base(parent)
    { }

    public FunctionStatement(BaseStatementAST? parent, KeywordToken keyword) : base(parent)
    {
        Heading = new StatementHeadingParseResult(keyword);
    }

    public FunctionStatement(BaseStatementAST? parent, StatementHeadingParseResult heading) : base(parent)
    {
        Heading = heading;
    }
}