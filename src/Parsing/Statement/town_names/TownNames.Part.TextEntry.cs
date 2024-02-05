using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal partial class TownNames
{
    private partial record struct Part
    {
        private readonly record struct TownNamesPartTextEntry(ExpressionAST? Call, BinaryOpToken? Comma = null)
        {
            public readonly ExpressionAST? Call = Call;
            public readonly BinaryOpToken? Comma = Comma;
        }
    }
}