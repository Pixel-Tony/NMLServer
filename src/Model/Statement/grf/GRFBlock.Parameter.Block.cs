using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Extensions;
using NMLServer.Model.Expression;
using NMLServer.Model.Lexis;
#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
#endif

namespace NMLServer.Model.Statement;


internal partial class GRFBlock
{
    private partial class Parameter
    {
        public readonly partial struct Block : IBlockContents<Block>, ISymbolSource, IFoldingRangeProvider
        {
            private readonly IdentifierToken? _name;
            private readonly BracketToken? _openingBracket;
            private readonly List<NMLAttribute>? _attributes;
            private readonly List<Names>? _names;
            private readonly BracketToken? _closingBracket;

            public readonly int End
                => _closingBracket?.End ?? (IHasEnd.LastOf(_names, _attributes, out var value)
                    ? value
                    : _openingBracket?.End ?? _name?.End ?? 0
                );

            public IdentifierToken? Symbol => _name;

            public static List<Block>? ParseSomeInBlock(ref ParsingState state, ref BracketToken? outerClosingBracket)
            {
                List<Block> content = [];
                while (state.CurrentToken is { } token)
                {
                    switch (token)
                    {
                        case IdentifierToken id:
                            content.Add(new Block(ref state, id, null));
                            break;

                        case KeywordToken { Kind: KeywordKind.BlockDefining }:
                            goto label_End;

                        case BracketToken { Bracket: '{' } openingBracket:
                            content.Add(new Block(ref state, null, openingBracket));
                            break;

                        case BracketToken { Bracket: '}' } closingBracket:
                            outerClosingBracket = closingBracket;
                            state.Increment();
                            goto label_End;

                        default:
                            state.AddUnexpected(token);
                            state.Increment();
                            break;
                    }
                }
            label_End:
                return content.ToMaybeList();
            }

            private Block(ref ParsingState state, IdentifierToken? maybeName, BracketToken? maybeOpeningBracket)
            {
                _name = maybeName;
                _openingBracket = maybeOpeningBracket;
                state.Increment();
                if (maybeOpeningBracket is not null)
                    goto label_Body;
                while (state.CurrentToken is { } token)
                {
                    switch (token)
                    {
                        case BracketToken { Bracket: '{' } openingBracket:
                            _openingBracket = openingBracket;
                            state.Increment();
                            goto label_Body;

                        case BracketToken { Bracket: '}' }:
                            return;

                        case KeywordToken { Kind: KeywordKind.BlockDefining }:
                            return;

                        default:
                            state.AddUnexpected(token);
                            state.Increment();
                            break;
                    }
                }
            label_Body:
                List<NMLAttribute> attributes = [];
                List<Names> names = [];

                IdentifierToken? key = null;
                ColonToken? colon = null;

                while (state.CurrentToken is { } token)
                {
                    switch (token)
                    {
                        case BracketToken { Bracket: '}' } closingBracket:
                            _closingBracket = closingBracket;
                            state.Increment();
                            goto label_End;

                        case IdentifierToken id when (key is null) & (colon is null):
                            key = id;
                            state.Increment();
                            continue;

                        case KeywordToken { Kind: KeywordKind.BlockDefining }:
                            goto label_End;

                        case BracketToken { Bracket: '{' } openingBracket:
                            names.Add(new Names(ref state, key, colon, openingBracket));
                            break;

                        case IdentifierToken
                            or UnitToken
                            or KeywordToken { Kind: KeywordKind.ExpressionUsable }
                            or UnaryOpToken
                            or BinaryOpToken
                            or TernaryOpToken
                            or BaseValueToken
                                when (key is not null) & (colon is not null):
                            {
                                var expr = ExpressionAST.TryParse(ref state);
                                var semicolon = state.ExpectSemicolon();
                                attributes.Add(new NMLAttribute(key, colon, expr, semicolon));
                            }
                            break;

                        case ColonToken c when (key is not null) & (colon is null):
                            colon = c;
                            state.Increment();
                            continue;

                        case SemicolonToken semicolon when (key is not null) & (colon is not null):
                            state.Increment();
                            attributes.Add(new NMLAttribute(key, colon, null, semicolon));
                            break;

                        default:
                            state.AddUnexpected(token);
                            state.Increment();
                            continue;
                    }
                    key = null;
                    colon = null;
                }
            label_End:
                if (key is not null | colon is not null)
                    attributes.Add(new NMLAttribute(key, colon, null, null));

                _attributes = attributes.ToMaybeList();
                _names = names.ToMaybeList();
            }

            public void ProvideFoldingRanges(in Stack<IFoldingRangeProvider> children,
                in List<FoldingRange> ranges, ref TokenStorage.PositionConverter converter)
            {
                IFoldingRangeProvider.RangeFromBrackets(_openingBracket, _closingBracket, in ranges, ref converter, true);
                IFoldingRangeProvider.Include(_names, in children);
            }

#if TREE_VISUALIZER_ENABLED
            public DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
            {
                throw new NotImplementedException();
            }
#endif
        }
    }
}