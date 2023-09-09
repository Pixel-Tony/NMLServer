using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement.Blocks;

internal record struct TrackTypeFallbackEntry(BaseValueToken? Identifier = null, ColonToken? Colon = null,
    ParentedExpression? Fallback = null, BinaryOpToken? Comma = null)
{
    public BaseValueToken? Identifier = Identifier;
    public ColonToken? Colon = Colon;
    public ParentedExpression? Fallback = Fallback;
    public BinaryOpToken? Comma = Comma;
}