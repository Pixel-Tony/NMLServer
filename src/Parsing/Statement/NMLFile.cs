using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

/* One ring to rule them all */
internal sealed class NMLFile
{
    private readonly List<BaseStatement> _children = new();
    public IEnumerable<BaseStatement> children => _children;

    public NMLFile(ParsingState state)
    {
        const int maxUnexpectedTokens = 400;

        for (var token = state.currentToken;
             state.unexpectedTokens.Count() <= maxUnexpectedTokens && token is not null;
             token = state.currentToken)
        {
            var statement = token switch
            {
                KeywordToken { Type: var type } keywordToken when Grammar.FunctionBlockKeywords.Contains(type)
                    => new FunctionLikeStatement(state, keywordToken),

                KeywordToken { Type: var type } keywordToken when Grammar.BlockKeywords.Contains(type)
                    => ParseBlockStatement(state, keywordToken),

                IdentifierToken
                    or KeywordToken { IsExpressionUsable: true }
                    or BracketToken { Bracket: not ('{' or '}') }
                    => new Assignment(state),

                _ => null
            };
            if (statement is null)
            {
                state.AddUnexpected(token);
                state.Increment();
                continue;
            }
            Logger.Log($"Parsed item of type {statement.GetType()}");
            _children.Add(statement);
        }
    }

    private static BaseStatement? ParseBlockStatement(ParsingState state, KeywordToken keyword)
    {
        switch (keyword.Type)
        {
            case KeywordType.SpriteSet:
                return new Spriteset(state, keyword);

            case KeywordType.Template:
                return new Template(state, keyword);

            case KeywordType.Grf:
                return new GRFBlock(state, keyword);

            case KeywordType.BaseCost:
                return new Basecost(state, keyword);

            case KeywordType.TramTypeTable:
            case KeywordType.RailTypeTable:
            case KeywordType.RoadTypeTable:
                return new TracktypeTable(state, keyword);

            case KeywordType.Switch:
                return new Switch(state, keyword);

            // These are not expected at top level
            case KeywordType.Param:
            case KeywordType.Var:
            case KeywordType.Property:
            case KeywordType.Graphics:
            case KeywordType.LiveryOverride:
            case KeywordType.Return:
                return null;

            // NOT YET IMPLEMENTED
            case KeywordType.Item:
            case KeywordType.CargoTable:
            case KeywordType.If:
            case KeywordType.Else:
            case KeywordType.While:
            case KeywordType.SnowLine:
            case KeywordType.SpriteGroup:
            case KeywordType.RandomSwitch:
            case KeywordType.Produce:
            case KeywordType.Replace:
            case KeywordType.ReplaceNew:
            case KeywordType.FontGlyph:
            case KeywordType.TownNames:
            case KeywordType.TileLayout:
            case KeywordType.SpriteLayout:
            case KeywordType.AlternativeSprites:
            case KeywordType.BaseGraphics:
            case KeywordType.RecolourSprite:
                return null;

            // keywords with function-like blocks are already processed
            case KeywordType.Error:
            case KeywordType.DisableItem:
            case KeywordType.Deactivate:
            case KeywordType.EngineOverride:
            case KeywordType.Sort:
            default:
                return null;
        }
    }
}