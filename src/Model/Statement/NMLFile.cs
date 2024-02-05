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
                    _children.Add(ParseBlockStatement(state, keywordToken));
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
                    _children.Add(ParseBlockStatement(state, keywordToken));
                    break;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
    }

    private static BaseStatement ParseBlockStatement(ParsingState state, KeywordToken keyword)
    {
        return keyword.Type switch
        {
            KeywordType.Grf => new GRFBlock(state, keyword),
            KeywordType.BaseCost => new Basecost(state, keyword),
            KeywordType.SpriteSet => new Spriteset(state, keyword),
            KeywordType.Template => new Template(state, keyword),
            KeywordType.TramTypeTable => new TracktypeTable(state, keyword),
            KeywordType.RailTypeTable => new TracktypeTable(state, keyword),
            KeywordType.RoadTypeTable => new TracktypeTable(state, keyword),
            KeywordType.Switch => new Switch(state, keyword),
            KeywordType.Item => new ItemBlock(state, keyword),
            KeywordType.Property => new ItemPropertyBlock(state, keyword),
            KeywordType.Graphics => new ItemGraphicsBlock(state, keyword),
            KeywordType.LiveryOverride => new ItemLiveryOverrideBlock(state, keyword),
            KeywordType.If => new IfBlock(state, keyword),
            KeywordType.Else => new ElseBlock(state, keyword),
            KeywordType.While => new WhileBlock(state, keyword),
            KeywordType.CargoTable => new Cargotable(state, keyword),
            KeywordType.Replace => new Replace(state, keyword),
            KeywordType.ReplaceNew => new ReplaceNew(state, keyword),
            KeywordType.BaseGraphics => new BaseGraphics(state, keyword),
            KeywordType.FontGlyph => new FontGlyph(state, keyword),
            KeywordType.AlternativeSprites => new AlternativeSprites(state, keyword),
            KeywordType.SpriteGroup => new SpriteGroup(state, keyword),
            KeywordType.RandomSwitch => new RandomSwitch(state, keyword),
            KeywordType.SpriteLayout => new SpriteLayout(state, keyword),
            KeywordType.TileLayout => new TileLayout(state, keyword),
            KeywordType.SnowLine => new SnowLine(state, keyword),
            KeywordType.Produce => new Produce(state, keyword),
            KeywordType.TownNames => new TownNames(state, keyword),
            KeywordType.RecolourSprite => new RecolourSprite(state, keyword),
            _ => throw new ArgumentOutOfRangeException(nameof(keyword.Type), "Unexpected keyword type")
        };
    }
}