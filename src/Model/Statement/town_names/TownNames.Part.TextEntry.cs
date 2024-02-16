using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal partial class TownNames
{
    private partial record struct Part
    {
        private readonly record struct TownNamesPartTextEntry(ExpressionAST Call, BinaryOpToken? Comma = null) : IHasEnd
        {
            public readonly ExpressionAST Call = Call;
            public readonly BinaryOpToken? Comma = Comma;

            public int end => Comma?.end ?? Call.end;
        }
    }
}