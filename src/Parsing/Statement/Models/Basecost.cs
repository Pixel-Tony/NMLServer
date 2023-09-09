using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement.Models;

internal class Basecost : BaseTitledStatement
{
    public BracketToken? OpeningBracket;
    public NMLAttribute[]? Body;
    public BracketToken? ClosingBracket;

    public Basecost(KeywordToken statementType) : base(statementType)
    { }
}