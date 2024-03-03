using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal partial class TownNames
{
    private partial struct Part
    {
        private readonly struct SubEntry(KeywordToken townNamesKeyword, ExpressionAST? args,
            BinaryOpToken? comma = null) : IHasEnd
        {
            public int end => comma?.end ?? (args?.end ?? townNamesKeyword.end);
        }
    }
}