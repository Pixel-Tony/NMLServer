using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal record struct ParameterNames(BracketToken OpeningBracket)
{
    public readonly BracketToken OpeningBracket = OpeningBracket;
    public Pair<NumericToken, ExpressionAST>[]? Items;
    public BracketToken? ClosingBracket;

    public override string ToString()
    {
        return $"      {OpeningBracket?.Bracket ?? '.'}\n"
               + $"      {(Items != null ? string.Join(",\n    ", Items) : ".")}\n"
               + $"      {ClosingBracket?.Bracket ?? '.'}";
    }
}