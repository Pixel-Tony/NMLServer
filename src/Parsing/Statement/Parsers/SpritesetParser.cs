using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Statements.Models;

namespace NMLServer.Parsing.Statements;

internal class SpritesetParser : ExpressionParser
{
    public static Spriteset Apply(KeywordToken alwaysSpriteset)
    {
        return SpriteHolderParser.Apply<Spriteset>(alwaysSpriteset);
    }
}