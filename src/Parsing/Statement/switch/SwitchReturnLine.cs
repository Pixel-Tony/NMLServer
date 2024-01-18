using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal record struct SwitchReturnLine(KeywordToken? ReturnKeyword, ExpressionAST? Value, SemicolonToken? Semicolon)
{
    public KeywordToken? ReturnKeyword = ReturnKeyword;
    public ExpressionAST? Value = Value;
    public SemicolonToken? Semicolon = Semicolon;
}