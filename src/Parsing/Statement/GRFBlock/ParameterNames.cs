using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal record struct ParameterNames(BracketToken OpeningBracket)
{
    public BracketToken OpeningBracket = OpeningBracket;
    public Pair<NumericToken, ExpressionAST>[]? Items;
    public BracketToken? ClosingBracket;
}