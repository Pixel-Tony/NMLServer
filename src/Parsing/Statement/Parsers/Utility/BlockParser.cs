using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Statements.Models;

namespace NMLServer.Parsing.Statements;

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

    protected static Statement ParseBlockStatement(KeywordToken keyword)
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

            case KeywordType.Param:
            case KeywordType.Var:
            case KeywordType.CargoTable:
            case KeywordType.If:
            case KeywordType.Else:
            case KeywordType.While:
            case KeywordType.Item:
            case KeywordType.Property:
            case KeywordType.Graphics:
            case KeywordType.LiveryOverride:
            case KeywordType.SnowLine:
            case KeywordType.SpriteGroup:
            case KeywordType.RandomSwitch:
            case KeywordType.Produce:
            case KeywordType.Error:
            case KeywordType.DisableItem:
            case KeywordType.Replace:
            case KeywordType.ReplaceNew:
            case KeywordType.FontGlyph:
            case KeywordType.Deactivate:
            case KeywordType.TownNames:
            case KeywordType.Return:
            case KeywordType.Exit:
            case KeywordType.TileLayout:
            case KeywordType.SpriteLayout:
            case KeywordType.AlternativeSprites:
            case KeywordType.BaseGraphics:
            case KeywordType.RecolourSprite:
            case KeywordType.EngineOverride:
            case KeywordType.Sort:
            default:
                Pointer = Max;
                return null!;
        }
    }
}