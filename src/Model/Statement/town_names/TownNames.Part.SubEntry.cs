using NMLServer.Model.Lexis;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal partial class TownNames
{
    private partial struct Part
    {
        private readonly struct SubEntry(KeywordToken townNames, ExpressionAST? args, BinaryOpToken? comma = null)
            : IHasEnd
        {
            public int End => comma?.End ?? args?.End ?? townNames.End;
        }
    }
}