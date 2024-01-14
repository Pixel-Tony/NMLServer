using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Statements.Models;

namespace NMLServer.Parsing.Statements;

internal class TemplateParser
{
    public static Template Apply(KeywordToken alwaysTemplate)
    {
        return SpriteHolderParser.Apply<Template>(alwaysTemplate);
    }
}