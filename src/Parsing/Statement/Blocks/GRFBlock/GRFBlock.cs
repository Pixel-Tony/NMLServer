using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class GRFBlock : BaseTitledStatement
{
    public BracketToken? _openingBracket;
    public NMLAttribute[]? _attributes;
    public GRFParameter[]? _parameters;
    public BracketToken? _closingBracket;

    public GRFBlock(BaseStatementAST? parent, KeywordToken alwaysGRF) : base(parent, alwaysGRF)
    { }
}