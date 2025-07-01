using System.Diagnostics.CodeAnalysis;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal struct StatementASTBuilder
{
    private InnerStatementNode? _current;
    private List<StatementAST> _children;

    public List<StatementAST> root { get; }

    public StatementASTBuilder()
    {
        _children = root = [];
    }

    public bool Make(ref ParsingState state) => Make(ref state, out _);

    public bool Make(ref ParsingState state, out StatementAST? output)
    {
        var token = state.currentToken;
        switch (token)
        {
            case null:
                output = null;
                return false;

            case KeywordToken { Kind: KeywordKind.BlockDefining } keyword:
                if (ParseStatement(_current, ref state, keyword, out var leaf, out var innerNode))
                {
                    _children.Add(leaf);
                    output = leaf;
                    return true;
                }
                _children.Add(innerNode);
                if (innerNode.Children is { } innerChildren)
                {
                    _current = innerNode;
                    _children = innerChildren;
                }
                output = innerNode;
                return true;

            case BracketToken { Bracket: '}' } closingBracket when _current is not null:
                _current.ClosingBracket = closingBracket;
                _current = _current.Parent;
                _children = _current?.Children ?? root;
                state.Increment();
                output = null;
                return true;

            case IdentifierToken:
            case KeywordToken { Kind: KeywordKind.ExpressionUsable }:
                output = new Assignment(ref state);
                _children.Add(output);
                return true;

            default:
                state.AddUnexpected(token);
                state.Increment();
                output = null;
                return true;
        }
    }

    private static bool ParseStatement(InnerStatementNode? parent, ref ParsingState state, KeywordToken keyword,
        [NotNullWhen(true)] out StatementAST? leaf, [NotNullWhen(false)] out InnerStatementNode? innerNode)
    {
        var type = keyword.Type;
        innerNode = type switch
        {
            KeywordType.If
                or KeywordType.Else
                or KeywordType.While
                => new FlowControlBlock(parent, ref state, keyword),

            KeywordType.Item => new ItemBlock(parent, ref state, keyword),
            _ => null
        };
        if (innerNode is not null)
        {
            leaf = null;
            return false;
        }
        leaf = type switch
        {
            KeywordType.Grf => new GRFBlock(ref state, keyword),
            KeywordType.Sort => new CallStatement(ref state, keyword),
            KeywordType.Error => new CallStatement(ref state, keyword),
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
            KeywordType.Deactivate => new CallStatement(ref state, keyword),
            KeywordType.ReplaceNew => new ReplaceNew(ref state, keyword),
            KeywordType.CargoTable => new Cargotable(ref state, keyword),
            KeywordType.TileLayout => new TileLayout(ref state, keyword),
            KeywordType.DisableItem => new CallStatement(ref state, keyword),
            KeywordType.SpriteGroup => new SpriteGroup(ref state, keyword),
            KeywordType.BaseGraphics => new BaseGraphics(ref state, keyword),
            KeywordType.RandomSwitch => new RandomSwitch(ref state, keyword),
            KeywordType.SpriteLayout => new SpriteLayout(ref state, keyword),
            KeywordType.TramTypeTable => new TracktypeTable(ref state, keyword),
            KeywordType.RailTypeTable => new TracktypeTable(ref state, keyword),
            KeywordType.RoadTypeTable => new TracktypeTable(ref state, keyword),
            KeywordType.EngineOverride => new CallStatement(ref state, keyword),
            KeywordType.RecolourSprite => new RecolourSprite(ref state, keyword),
            KeywordType.LiveryOverride => new ItemLiveryOverrideBlock(ref state, keyword),
            KeywordType.AlternativeSprites => new AlternativeSprites(ref state, keyword),
            _ => throw new ArgumentOutOfRangeException(nameof(keyword), keyword.Type, "Unexpected keyword type")
        };
        return true;
    }
}