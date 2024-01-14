using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statements.Models;

internal class Basecost : TitledStatement
{
    public BracketToken? OpeningBracket;
    public NMLAttribute[]? Body;
    public BracketToken? ClosingBracket;

    public Basecost(KeywordToken statementType) : base(statementType)
    { }
}