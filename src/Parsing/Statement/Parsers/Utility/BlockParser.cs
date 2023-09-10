using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Statement;
using NMLServer.Parsing.Statement.Models;

namespace NMLServer.Parsing;

internal class BlockParser : HeadingParser
{
    protected static FunctionStatement ParseFunctionStatement(KeywordToken keyword)
    {
        var result = new FunctionStatement();
        ParseHeading(keyword, out result.Heading);

        while (areTokensLeft)
        {
            var current = Tokens[Pointer];
            switch (current)
            {
                case SemicolonToken semicolon:
                    result.Semicolon = semicolon;
                    Pointer++;
                    return result;

                case BracketToken:
                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                    return result;

                default:
                    UnexpectedTokens.Add(current);
                    break;
            }
            Pointer++;
        }
        return result;
    }

    protected static BaseStatement ParseBlockStatement(KeywordToken keyword)
    {
        switch (keyword.Type)
        {
            case KeywordType.Grf:
                return GRFBlockParser.Apply(keyword);

            case KeywordType.BaseCost:
                return BasecostParser.Apply(keyword);

            case KeywordType.TramTypeTable:
            case KeywordType.RailTypeTable:
            case KeywordType.RoadTypeTable:
                return TracktypeTableParser.Apply(keyword);

            case KeywordType.SpriteSet:
                return SpritesetParser.Apply(keyword);

            case KeywordType.Template:
                return TemplateParser.Apply(keyword);

            case KeywordType.Switch:
                return SwitchParser.Apply(keyword);

            default:
                Pointer = Max;
                return null!;
        }
    }
}