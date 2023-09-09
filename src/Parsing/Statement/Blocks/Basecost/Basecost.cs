using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement.Blocks;

internal class Basecost : BaseTitledStatement
{
    public BracketToken? OpeningBracket;
    public NMLAttribute[]? Body;
    public BracketToken? ClosingBracket;

    public Basecost(KeywordToken statementType) : base(statementType)
    { }
}