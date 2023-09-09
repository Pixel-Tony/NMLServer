using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement.Blocks;

internal class TracktypeTable : BaseTitledStatement
{
    public BracketToken? OpeningBracket;
    public (BaseValueToken? identifier, BinaryOpToken? comma)[]? StandaloneLabels;
    public TrackTypeFallbackEntry[]? FallbackLabels;
    public BracketToken? ClosingBracket;

    public TracktypeTable(KeywordToken keyword) : base(keyword)
    { }
}