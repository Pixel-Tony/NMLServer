using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement.Models;

internal class Switch : BaseStatement
{
    public StatementHeading Heading;
    public BracketToken? OpeningBracket;
    public SwitchLine[]? Body;
    public BracketToken? ClosingBracket;
}