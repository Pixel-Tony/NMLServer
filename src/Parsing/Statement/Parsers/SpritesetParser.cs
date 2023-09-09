using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Statement.Models;

namespace NMLServer.Parsing.Statement;

internal class SpritesetParser : ExpressionParser
{
    public static Spriteset Apply(KeywordToken alwaysSpriteset)
    {
        Spriteset result = new();
        SpriteHolderParser.Apply(alwaysSpriteset, result);
        return result;
    }
}