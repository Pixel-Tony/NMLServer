using System.Runtime.CompilerServices;
using NMLServer.Lexing;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer.Model.Statement;

/* One ring to rule them all */
internal readonly struct NMLFile
{
    private readonly List<BaseStatement> _children = new();
    private readonly Dictionary<IdentifierToken, List<ISymbolSource>> _definedSymbols;

    private static IIdentifierTokenComparer _currentComparer = null!;

    private readonly List<Token>? _tokens;
    public IEnumerable<Token> tokens => _tokens!;
    public IReadOnlyList<Token> unexpectedTokens { get; } = null!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NMLFile Make(List<Token> tokens, IIdentifierTokenComparer comparer)
    {
        _currentComparer = comparer;
        return new NMLFile(tokens);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private NMLFile(List<Token> tokens) : this(new ParsingState(tokens), out _, false)
    {
        _tokens = tokens;
    }

    public NMLFile(ParsingState state, out BracketToken? expectedClosingBracket, bool inner)
    {
        if (!inner)
        {
            unexpectedTokens = state.unexpectedTokens;
        }
        _definedSymbols = new Dictionary<IdentifierToken, List<ISymbolSource>>(_currentComparer);

        for (var token = state.currentToken; token is not null; token = state.currentToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '}' } closingBracket when inner:
                    expectedClosingBracket = closingBracket;
                    state.Increment();
                    return;

                case IdentifierToken:
                case KeywordToken { Kind: KeywordKind.ExpressionUsable }:
                case BracketToken { Bracket: not ('{' or '}') }:
                    Assignment assignment = new(state);
                    _children.Add(assignment);
                    var parameter = assignment.symbol;
                    if (parameter is null)
                    {
                        break;
                    }
                    if (_definedSymbols.TryGetValue(parameter, out var paramMentions))
                    {
                        paramMentions.Add(assignment);
                        break;
                    }
                    _definedSymbols[parameter] = new List<ISymbolSource>(1) { assignment };
                    break;

                case KeywordToken { Kind: KeywordKind.FunctionBlockDefining } keywordToken:
                    _children.Add(new FunctionLikeStatement(state, keywordToken));
                    break;

                case KeywordToken { Kind: KeywordKind.BlockDefining } keywordToken:
                {
                    var child = ParseBlockStatement(state, keywordToken);
                    _children.Add(child);
                    if (child is not ISymbolSource { symbol: { } element } canDefineElement)
                    {
                        break;
                    }
                    if (_definedSymbols.TryGetValue(element, out var list))
                    {
                        list.Add(canDefineElement);
                        break;
                    }
                    _definedSymbols[element] = new List<ISymbolSource>(1) { canDefineElement };
                    break;
                }

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        expectedClosingBracket = null;
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
            _ => throw new ArgumentOutOfRangeException(nameof(keyword.Type), "Unexpected keyword type")
        };
    }
}