using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

/* One ring to rule them all */
internal readonly struct NMLFile
{
    private readonly List<BaseStatement> _children = new();

    public NMLFile(ParsingState state)
    {
        const int maxUnexpectedTokens = 100;

        for (var token = state.currentToken;
             state.unexpectedTokens.Count <= maxUnexpectedTokens && token is not null;
             token = state.currentToken)
        {
            switch (token)
            {
                case IdentifierToken:
                case KeywordToken { Kind: KeywordKind.ExpressionUsable }:
                case BracketToken { Bracket: not ('{' or '}') }:
                    _children.Add(new Assignment(state));
                    break;

                case KeywordToken { Kind: KeywordKind.FunctionBlockDefining } keywordToken:
                    _children.Add(new FunctionLikeStatement(state, keywordToken));
                    break;

                case KeywordToken { Kind: KeywordKind.BlockDefining } keywordToken:
                    var statement = ParseBlockStatement(state, keywordToken);
                    if (statement is null)
                    {
                        goto default;
                    }
                    _children.Add(statement);
                    break;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
    }

    public NMLFile(ParsingState state, ref BracketToken? expectedClosingBracket)
    {
        const int maxUnexpectedTokens = 100;
        var unexpectedTokens = state.unexpectedTokens;

        for (var token = state.currentToken;
             token is not null && unexpectedTokens.Count <= maxUnexpectedTokens;
             token = state.currentToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' } closingBracket:
                    expectedClosingBracket = closingBracket;
                    state.Increment();
                    return;

                case IdentifierToken:
                case KeywordToken { Kind: KeywordKind.ExpressionUsable }:
                case BracketToken { Bracket: not ('{' or '}') }:
                    _children.Add(new Assignment(state));
                    break;

                case KeywordToken { Kind: KeywordKind.FunctionBlockDefining } keywordToken:
                    _children.Add(new FunctionLikeStatement(state, keywordToken));
                    break;

                case KeywordToken keywordToken:
                    var statement = ParseBlockStatement(state, keywordToken);
                    if (statement is null)
                    {
                        goto default;
                    }
                    _children.Add(statement);
                    break;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
    }

    private static BaseStatement? ParseBlockStatement(ParsingState state, KeywordToken keyword)
    {
        switch (keyword.Type)
        {
            case KeywordType.Grf:
                return new GRFBlock(state, keyword);

            case KeywordType.BaseCost:
                return new Basecost(state, keyword);

            case KeywordType.SpriteSet:
                return new Spriteset(state, keyword);

            case KeywordType.Template:
                return new Template(state, keyword);

            case KeywordType.TramTypeTable:
            case KeywordType.RailTypeTable:
            case KeywordType.RoadTypeTable:
                return new TracktypeTable(state, keyword);

            case KeywordType.Switch:
                return new Switch(state, keyword);

            case KeywordType.Item:
                return new ItemBlock(state, keyword);

            case KeywordType.Property:
                return new ItemPropertyBlock(state, keyword);

            case KeywordType.Graphics:
                return new ItemGraphicsBlock(state, keyword);

            case KeywordType.LiveryOverride:
                return new ItemLiveryOverrideBlock(state, keyword);

            case KeywordType.If:
                return new IfBlock(state, keyword);

            case KeywordType.Else:
                return new ElseBlock(state, keyword);

            case KeywordType.While:
                return new WhileBlock(state, keyword);

            case KeywordType.CargoTable:
                return new Cargotable(state, keyword);

            case KeywordType.Replace:
                return new Replace(state, keyword);

            case KeywordType.ReplaceNew:
                return new ReplaceNew(state, keyword);

            case KeywordType.BaseGraphics:
                return new BaseGraphics(state, keyword);

            case KeywordType.FontGlyph:
                return new FontGlyph(state, keyword);

            case KeywordType.AlternativeSprites:
                return new AlternativeSprites(state, keyword);

            case KeywordType.SpriteGroup:
                return new SpriteGroup(state, keyword);

            case KeywordType.RandomSwitch:
                return new RandomSwitch(state, keyword);

            case KeywordType.SpriteLayout:
                return new SpriteLayout(state, keyword);

            case KeywordType.TileLayout:
                return new TileLayout(state, keyword);

            case KeywordType.SnowLine:
                return new SnowLine(state, keyword);

            case KeywordType.Produce:
                return new Produce(state, keyword);

            // TODO
            case KeywordType.TownNames:
            case KeywordType.RecolourSprite:
                return null;

            default:
                return null;
        }
    }
}