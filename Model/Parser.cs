using NMLServer.Extensions;
using NMLServer.Model.Grammar;
using NMLServer.Model.Statements;
using NMLServer.Model.Statements.Blocks;
using NMLServer.Model.Statements.Procedures;
using NMLServer.Model.Tokens;

namespace NMLServer.Model;

internal struct Parser
{
    private BaseParentStatement? _current;
    private List<BaseStatement> _children;

    public List<BaseStatement> Root { get; }

    public Parser()
    {
        _children = Root = [];
    }

    // public bool Make(ref ParsingState state, out BaseStatement? output)
    // {
    //     var token = state.CurrentToken;
    //     switch (token)
    //     {
    //         case null:
    //             output = null;
    //             return false;

    //         case KeywordToken { IsDefiningStatement: true } keywordToken:
    //             output = ParseStatement(_current, ref state, keywordToken, out var parent);
    //             _children.Add(output);
    //             if (parent is { Children: { } innerChildren })
    //             {
    //                 _current = parent;
    //                 _children = innerChildren;
    //             }
    //             return true;

    //         case BracketToken { Bracket: '}' } closingBracket when _current is not null:
    //             _current.ClosingBracket = closingBracket;
    //             _current.Children = _current.Children!.ToMaybeList();
    //             _current = _current.Parent;
    //             _children = _current?.Children ?? Root;
    //             state.Increment();
    //             output = null;
    //             return true;

    //         case IdentifierToken:
    //         case KeywordToken { IsExpressionUsable: true }:
    //             output = new Assignment(ref state);
    //             _children.Add(output);
    //             return true;

    //         default:
    //             state.AddUnexpected(token);
    //             state.Increment();
    //             output = null;
    //             return true;
    //     }
    // }

    public static BaseStatement ParseStatement(BaseParentStatement? parent, ref ParsingState state, KeywordToken keywordToken,
        out BaseParentStatement? innerNode)
    {
        var keyword = keywordToken.Keyword;
        BaseStatement? node = innerNode = keyword switch
        {
            Keyword.If => new FlowControlBlock(parent, ref state, keywordToken),
            Keyword.Else => new FlowControlBlock(parent, ref state, keywordToken),
            Keyword.While => new FlowControlBlock(parent, ref state, keywordToken),
            Keyword.Item => new ItemBlock(parent, ref state, keywordToken),
            _ => null
        };
        return node ?? keyword switch
        {
            Keyword.AlternativeSprites => new AlternativeSprites(ref state, keywordToken),
            Keyword.BaseCost => new Basecost(ref state, keywordToken),
            Keyword.BaseGraphics => new BaseGraphics(ref state, keywordToken),
            Keyword.CargoTable => new Cargotable(ref state, keywordToken),
            Keyword.Const => new Constant(ref state, keywordToken),
            Keyword.Deactivate => new Procedure(ref state, keywordToken),
            Keyword.DisableItem => new Procedure(ref state, keywordToken),
            Keyword.EngineOverride => new Procedure(ref state, keywordToken),
            Keyword.Error => new Procedure(ref state, keywordToken),
            Keyword.FontGlyph => new FontGlyph(ref state, keywordToken),
            Keyword.Graphics => new GraphicsBlock(ref state, keywordToken),
            Keyword.Grf => new GRF(ref state, keywordToken),
            Keyword.LiveryOverride => new LiveryOverrideBlock(ref state, keywordToken),
            Keyword.Produce => new Produce(ref state, keywordToken),
            Keyword.Property => new PropertyBlock(ref state, keywordToken),
            Keyword.RailTypeTable => new TracktypeTable(ref state, keywordToken),
            Keyword.RandomSwitch => new RandomSwitch(ref state, keywordToken),
            Keyword.RecolourSprite => new RecolourSprite(ref state, keywordToken),
            Keyword.Replace => new Replace(ref state, keywordToken),
            Keyword.ReplaceNew => new ReplaceNew(ref state, keywordToken),
            Keyword.RoadTypeTable => new TracktypeTable(ref state, keywordToken),
            Keyword.SnowLine => new SnowLine(ref state, keywordToken),
            Keyword.Sort => new Procedure(ref state, keywordToken),
            Keyword.SpriteGroup => new SpriteGroup(ref state, keywordToken),
            Keyword.SpriteLayout => new SpriteLayout(ref state, keywordToken),
            Keyword.SpriteSet => new Spriteset(ref state, keywordToken),
            Keyword.Switch => new Switch(ref state, keywordToken),
            Keyword.Template => new Template(ref state, keywordToken),
            Keyword.TileLayout => new TileLayout(ref state, keywordToken),
            Keyword.TownNames => new TownNames(ref state, keywordToken),
            Keyword.TramTypeTable => new TracktypeTable(ref state, keywordToken),
            _ => throw new ArgumentOutOfRangeException(nameof(keywordToken), keywordToken.Keyword, "Unexpected keyword type")
        };
    }
}