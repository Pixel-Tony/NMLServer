using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal partial class TownNames
{
    private partial struct Part
    {
        private readonly struct TownNamesPartTextEntry(ExpressionAST call, BinaryOpToken? comma = null) : IHasEnd
        {
            public int end => comma?.end ?? call.end;
        }
    }
}