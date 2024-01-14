using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statements.Models;

internal class TracktypeTable : TitledStatement
{
    public BracketToken? OpeningBracket;
    public (BaseValueToken? identifier, BinaryOpToken? comma)[]? StandaloneLabels;
    public TrackTypeFallbackEntry[]? FallbackLabels;
    public BracketToken? ClosingBracket;

    public TracktypeTable(KeywordToken keyword) : base(keyword)
    { }
}