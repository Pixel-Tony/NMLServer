using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Extensions;
using NMLServer.Model.Expressions;
using NMLServer.Model.Tokens;
using DotNetGraph.Core;

namespace NMLServer.Model.Statements.Blocks;


internal partial class GRF
{
    private partial class Parameter
    {
        public readonly partial struct Setting : IBlockContents<Setting>, IFoldingRangeProvider
        {
            private readonly IdentifierToken? _name;
            private readonly BracketToken? _openingBracket;
            private readonly List<PropertySetting>? _attributes;
            private readonly List<Names>? _names;
            private readonly BracketToken? _closingBracket;

            public readonly int End
                => _closingBracket?.End ?? (_names, _attributes).LastOf() ?? _openingBracket?.End ?? _name!.End;

            public static List<Setting>? ParseSomeInBlock(ref ParsingState state, ref BracketToken? outerClosingBracket)
            {
                List<Setting> content = [];
                while (state.CurrentToken is { } token)
                {
                    switch (token)
                    {
                        case IdentifierToken id:
                            content.Add(new Setting(ref state, id, null));
                            break;

                        case KeywordToken { IsDefiningStatement: true }:
                            goto label_End;

                        case BracketToken { Bracket: '{' } openingBracket:
                            content.Add(new Setting(ref state, null, openingBracket));
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

            private Setting(ref ParsingState state, IdentifierToken? maybeName, BracketToken? maybeOpeningBracket)
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
                        case KeywordToken { IsDefiningStatement: true }:
                            return;

                        default:
                            state.AddUnexpected(token);
                            state.Increment();
                            break;
                    }
                }
            label_Body:
                List<PropertySetting> attributes = [];
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

                        case KeywordToken { IsDefiningStatement: true }:
                            goto label_End;

                        case BracketToken { Bracket: '{' } openingBracket:
                            names.Add(new Names(ref state, key, colon, openingBracket));
                            break;

                        case IdentifierToken
                            or UnitToken
                            or KeywordToken { IsExpressionUsable: true }
                            or UnaryOpToken
                            or BinaryOpToken
                            or TernaryOpToken
                            or BaseValueToken
                                when (key is not null) & (colon is not null):
                            {
                                var expr = BaseExpression.TryParse(ref state);
                                var semicolon = state.ExpectSemicolon();
                                attributes.Add(new PropertySetting(key, colon, expr, semicolon));
                            }
                            break;

                        case ColonToken c when (key is not null) & (colon is null):
                            colon = c;
                            state.Increment();
                            continue;

                        case SemicolonToken semicolon when (key is not null) & (colon is not null):
                            state.Increment();
                            attributes.Add(new PropertySetting(key, colon, null, semicolon));
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
                    attributes.Add(new PropertySetting(key, colon, null, null));

                _attributes = attributes.ToMaybeList();
                _names = names.ToMaybeList();
            }

            public void ProvideFoldingRanges(List<FoldingRange> ranges, ref PositionConverter converter)
                => IFoldingRangeProvider.RangeWithInnerRanges(_openingBracket, _closingBracket, _names, ranges, ref converter);

            public DotNode Visualize(DotGraph graph, DotNode parent, StringView ctx)
            {
                var n = VizExtensions.MakeNode(graph, parent, "Block")
                    .WithStmtFeatures();
                _name.MaybeVisualize(graph, n, ctx);
                _openingBracket.MaybeVisualize(graph, n, ctx);
                foreach (var attr in _attributes ?? [])
                    attr.Visualize(graph, n, ctx);
                foreach (var _ in _names ?? [])
                    VizExtensions.MakeNode(graph, n, "Names");
                _closingBracket.MaybeVisualize(graph, n, ctx);
                return n;
            }
        }
    }
}