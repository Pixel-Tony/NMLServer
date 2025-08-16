using NMLServer.Extensions;
using NMLServer.Model.Tokens;
using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using DotNetGraph.Core;

namespace NMLServer.Model.Statements.Blocks;

internal sealed partial class SpriteLayout
{
    public readonly struct Entry : IBlockContents<Entry>, IFoldingRangeProvider
    {
        private readonly IdentifierToken? _identifier;
        private readonly BracketToken? _openingBracket;
        private readonly List<PropertySetting>? _attributes;
        private readonly BracketToken? _closingBracket;

        public int End => _closingBracket?.End ?? _attributes?[^1].End ?? _openingBracket?.End ?? _identifier!.End;

        public static List<Entry>? ParseSomeInBlock(ref ParsingState state, ref BracketToken? closingBracket)
        {
            List<Entry> entries = [];
            while (state.CurrentToken is { } token)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '{' } openingBracket:
                        entries.Add(new Entry(ref state, openingBracket));
                        break;

                    case BracketToken { Bracket: '}' } expectedClosingBracket:
                        closingBracket = expectedClosingBracket;
                        state.Increment();
                        goto label_End;

                    case IdentifierToken identifierToken:
                        entries.Add(new Entry(ref state, identifierToken));
                        break;

                    case KeywordToken { IsDefiningStatement: true }:
                        goto label_End;

                    default:
                        state.AddUnexpected(token);
                        state.Increment();
                        break;
                }
            }
        label_End:
            return entries.ToMaybeList();
        }

        private Entry(ref ParsingState state, BracketToken openingBracket)
        {
            _openingBracket = openingBracket;
            state.Increment();
            _attributes = PropertySetting.ParseSomeInBlock(ref state, ref _closingBracket);
        }

        private Entry(ref ParsingState state, IdentifierToken identifier)
        {
            _identifier = identifier;
            state.Increment();
            while (state.CurrentToken is { } token)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '{' } openingBracket:
                        _openingBracket = openingBracket;
                        state.Increment();
                        _attributes = PropertySetting.ParseSomeInBlock(ref state, ref _closingBracket);
                        return;

                    case BracketToken { Bracket: '}' } closingBracket:
                        _closingBracket = closingBracket;
                        state.Increment();
                        return;

                    case KeywordToken { IsDefiningStatement: true }:
                        return;

                    default:
                        state.AddUnexpected(token);
                        state.Increment();
                        break;
                }
            }
        }

        public void ProvideFoldingRanges(List<FoldingRange> ranges, ref PositionConverter converter)
            => IFoldingRangeProvider.RangeFromBrackets(_openingBracket, _closingBracket, ranges, ref converter);

        public DotNode Visualize(DotGraph graph, DotNode parent, StringView ctx)
        {
            var n = VizExtensions.MakeNode(graph, parent, "Entry").WithStmtFeatures();
            _identifier.MaybeVisualize(graph, n, ctx);
            _openingBracket.MaybeVisualize(graph, n, ctx);
            foreach (var a in _attributes ?? [])
                a.Visualize(graph, n, ctx);
            _closingBracket.MaybeVisualize(graph, n, ctx);
            return n;
        }
    }
}