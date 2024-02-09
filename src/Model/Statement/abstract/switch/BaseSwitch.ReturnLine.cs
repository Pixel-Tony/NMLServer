using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal partial class BaseSwitch
{
    private protected record struct ReturnLine(KeywordToken? ReturnKeyword, ExpressionAST? Value,
        SemicolonToken? Semicolon)
    {
        public KeywordToken? ReturnKeyword = ReturnKeyword;
        public ExpressionAST? Value = Value;
        public SemicolonToken? Semicolon = Semicolon;
    }
}