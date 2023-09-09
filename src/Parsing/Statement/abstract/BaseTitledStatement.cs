using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal abstract class BaseTitledStatement : BaseStatement
{
    protected readonly KeywordToken Type;

    protected BaseTitledStatement(KeywordToken statementType) =>
        Type = statementType;
}