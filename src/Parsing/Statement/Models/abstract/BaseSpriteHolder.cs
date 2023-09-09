using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement.Models;

internal abstract class BaseSpriteHolder : BaseStatement
{
    public StatementHeading Heading;
    public BracketToken? OpeningBracket;
    public ExpressionAST[]? Content;
    public BracketToken? ClosingBracket;
}