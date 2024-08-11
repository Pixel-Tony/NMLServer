using System.Diagnostics.CodeAnalysis;
using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal abstract class StatementAST : IHasEnd
{
    public abstract int start { get; }
    public abstract int end { get; }

    public static List<StatementAST>? Build(ref ParsingState state)
    {
        List<StatementAST> root = [];
        var children = root;
        InnerStatementNode? current = null;

        for (var token = state.currentToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case KeywordToken { Kind: KeywordKind.FunctionBlockDefining } keyword:
                    children.Add(new CallStatement(ref state, keyword));
                    break;

                case KeywordToken { Kind: KeywordKind.BlockDefining } keyword:
                    if (ParseBlockStatement(current, ref state, keyword, out var leafNode, out var innerNode))
                    {
                        children.Add(leafNode);
                        break;
                    }
                    children.Add(innerNode);
                    if (innerNode.Children is not null)
                    {
                        current = innerNode;
                        children = innerNode.Children;
                    }
                    break;

                case BracketToken { Bracket: '}' } closingBracket when current is not null:
                    current.ClosingBracket = closingBracket;
                    current = current.Parent;
                    children = current?.Children ?? root;
                    state.Increment();
                    break;

                case IdentifierToken:
                case KeywordToken { Kind: KeywordKind.ExpressionUsable }:
                    children.Add(new Assignment(ref state));
                    break;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        return root.ToMaybeList();
    }

    private static bool ParseBlockStatement(InnerStatementNode? parent, ref ParsingState state, KeywordToken keyword,
        [NotNullWhen(true)] out StatementAST? leaf, [NotNullWhen(false)] out InnerStatementNode? innerNode)
    {
        innerNode = keyword.Type switch
        {
            KeywordType.If => new IfBlock(parent, ref state, keyword),
            KeywordType.Item => new ItemBlock(parent, ref state, keyword),
            KeywordType.Else => new ElseBlock(parent, ref state, keyword),
            KeywordType.While => new WhileBlock(parent, ref state, keyword),
            _ => null
        };
        if (innerNode is not null)
        {
            leaf = null;
            return false;
        }
        leaf = keyword.Type switch
        {
            KeywordType.Grf => new GRFBlock(ref state, keyword),
            KeywordType.Switch => new Switch(ref state, keyword),
            KeywordType.Produce => new Produce(ref state, keyword),
            KeywordType.Replace => new Replace(ref state, keyword),
            KeywordType.BaseCost => new Basecost(ref state, keyword),
            KeywordType.Template => new Template(ref state, keyword),
            KeywordType.SnowLine => new SnowLine(ref state, keyword),
            KeywordType.Property => new ItemPropertyBlock(ref state, keyword),
            KeywordType.Graphics => new ItemGraphicsBlock(ref state, keyword),
            KeywordType.SpriteSet => new Spriteset(ref state, keyword),
            KeywordType.FontGlyph => new FontGlyph(ref state, keyword),
            KeywordType.TownNames => new TownNames(ref state, keyword),
            KeywordType.ReplaceNew => new ReplaceNew(ref state, keyword),
            KeywordType.CargoTable => new Cargotable(ref state, keyword),
            KeywordType.TileLayout => new TileLayout(ref state, keyword),
            KeywordType.SpriteGroup => new SpriteGroup(ref state, keyword),
            KeywordType.BaseGraphics => new BaseGraphics(ref state, keyword),
            KeywordType.RandomSwitch => new RandomSwitch(ref state, keyword),
            KeywordType.SpriteLayout => new SpriteLayout(ref state, keyword),
            KeywordType.TramTypeTable => new TracktypeTable(ref state, keyword),
            KeywordType.RailTypeTable => new TracktypeTable(ref state, keyword),
            KeywordType.RoadTypeTable => new TracktypeTable(ref state, keyword),
            KeywordType.RecolourSprite => new RecolourSprite(ref state, keyword),
            KeywordType.LiveryOverride => new ItemLiveryOverrideBlock(ref state, keyword),
            KeywordType.AlternativeSprites => new AlternativeSprites(ref state, keyword),
            _ => throw new ArgumentOutOfRangeException(nameof(keyword), keyword.Type, "Unexpected keyword type")
        };
        return true;
    }
}