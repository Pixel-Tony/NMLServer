using NMLServer.Model.Tokens;
using NMLServer.Model.Expressions;

namespace NMLServer.Model.Statements.Blocks;

internal partial class TownNames
{
    private partial struct Part
    {
        private readonly struct TextEntry(BaseExpression call, BinaryOpToken? comma = null) : IHasEnd
        {
            public int End => comma?.End ?? call.End;
        }
    }
}