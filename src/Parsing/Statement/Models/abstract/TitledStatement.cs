using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statements.Models;

internal abstract class TitledStatement : Statement
{
    protected readonly KeywordToken Type;

    protected TitledStatement(KeywordToken statementType) =>
        Type = statementType;
}