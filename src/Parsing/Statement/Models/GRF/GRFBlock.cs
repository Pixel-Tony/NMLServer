using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statements.Models;

internal class GRFBlock : TitledStatement
{
    public BracketToken? OpeningBracket;
    public NMLAttribute[]? Attributes;
    public GRFParameter[]? Parameters;
    public BracketToken? ClosingBracket;

    public GRFBlock(KeywordToken alwaysGRF) : base(alwaysGRF)
    { }
}