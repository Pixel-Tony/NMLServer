using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal partial class TownNames
{
    private partial record struct Part
    {
        private readonly record struct SubEntry(KeywordToken TownNamesKeyword, ExpressionAST? Args,
            BinaryOpToken? Comma = null)
        {
            public readonly KeywordToken TownNamesKeyword = TownNamesKeyword;
            public readonly ExpressionAST? Args = Args;
            public readonly BinaryOpToken? Comma = Comma;
        }
    }
}