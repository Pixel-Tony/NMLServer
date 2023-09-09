using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Statement.Models;

namespace NMLServer.Parsing.Statement;

internal class TemplateParser
{
    public static Template Apply(KeywordToken alwaysTemplate)
    {
        return SpriteHolderParser.Apply<Template>(alwaysTemplate);
    }
}