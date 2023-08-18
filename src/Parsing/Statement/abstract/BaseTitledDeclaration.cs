using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal abstract class BaseTitledDeclaration : BaseTitledStatement
{
    protected BaseTitledDeclaration(NMLFileRoot? parent, KeywordToken statementType) : base(parent, statementType)
    { }
}