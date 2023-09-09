using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement.Models;

internal class GRFBlock : BaseTitledStatement
{
    public BracketToken? OpeningBracket;
    public NMLAttribute[]? Attributes;
    public GRFParameter[]? Parameters;
    public BracketToken? ClosingBracket;

    public GRFBlock(KeywordToken alwaysGRF) : base(alwaysGRF)
    { }
}