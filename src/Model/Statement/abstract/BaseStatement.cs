using System.Runtime.CompilerServices;
using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal abstract class BaseStatement : IAllowsParseInsideBlock<BaseStatement>
{
    public abstract int start { get; }
    public abstract int end { get; }

    public static List<BaseStatement>? ParseSomeInBlock(ParsingState state, ref BracketToken? closingBracket)
        => ParseSomeInBlock(state, ref closingBracket, true);

    public static List<BaseStatement>? ParseSomeInBlock(ParsingState state, ref BracketToken? closingBracket,
        bool isInner)
    {
        List<BaseStatement> children = [];
        for (var token = state.currentToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case KeywordToken { Kind: KeywordKind.FunctionBlockDefining } keywordToken:
                    children.Add(new FunctionLikeStatement(state, keywordToken));
                    break;

                case KeywordToken { Kind: KeywordKind.BlockDefining } keywordToken:
                    children.Add(ParseBlockStatement(state, keywordToken));
                    break;

                case BracketToken { Bracket: '}' } expectedClosingBracket when isInner:
                    closingBracket = expectedClosingBracket;
                    state.Increment();
                    return children;

                case IdentifierToken:
                case KeywordToken { Kind: KeywordKind.ExpressionUsable }:
                case BracketToken { Bracket: not ('{' or '}') }:
                    children.Add(new Assignment(state));
                    break;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        return children.ToMaybeList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            _ => throw new ArgumentOutOfRangeException(nameof(keyword), keyword.Type, "Unexpected keyword type")
        };
    }
}