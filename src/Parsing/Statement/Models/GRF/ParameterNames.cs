using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement.Models;

internal record struct ParameterNames(BracketToken OpeningBracket)
{
    public readonly BracketToken OpeningBracket = OpeningBracket;
    public Pair<NumericToken, ExpressionAST>[]? Items;
    public BracketToken? ClosingBracket;
}