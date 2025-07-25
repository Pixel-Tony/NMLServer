#if TREE_VISUALIZER_ENABLED
using DotNetGraph.Core;
using NMLServer.Extensions.DotNetGraph;
#endif
using NMLServer.Extensions;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal sealed partial class SpriteLayout
{
    public readonly struct Entry : IAllowsParseInsideBlock<Entry>
    {
        private readonly IdentifierToken? _identifier;
        private readonly BracketToken? _openingBracket;
        private readonly List<NMLAttribute>? _attributes;
        private readonly BracketToken? _closingBracket;

        public int End => _closingBracket?.End ?? _attributes?[^1].End ?? _openingBracket?.End ?? _identifier!.End;

        private Entry(ref ParsingState state, BracketToken openingBracket)
        {
            _openingBracket = openingBracket;
            state.Increment();
            _attributes = NMLAttribute.ParseSomeInBlock(ref state, ref _closingBracket);
        }

        private Entry(ref ParsingState state, IdentifierToken identifier)
        {
            _identifier = identifier;
            while (state.NextToken is { } token)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '{' } openingBracket:
                        _openingBracket = openingBracket;
                        state.Increment();
                        _attributes = NMLAttribute.ParseSomeInBlock(ref state, ref _closingBracket);
                        return;

                    case BracketToken { Bracket: '}' } closingBracket:
                        _closingBracket = closingBracket;
                        state.Increment();
                        return;

                    case KeywordToken { Kind: KeywordKind.BlockDefining }:
                        return;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }
        }

        static List<Entry>? IAllowsParseInsideBlock<Entry>.ParseSomeInBlock(ref ParsingState state,
            ref BracketToken? closingBracket)
        {
            List<Entry> entries = [];
            for (var token = state.CurrentToken; token is not null; token = state.CurrentToken)
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

                    case KeywordToken { Kind: KeywordKind.BlockDefining }:
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

#if TREE_VISUALIZER_ENABLED
        public DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        {
            var n = VizExtensions.MakeNode(graph, parent, "Entry").WithStmtFeatures();
            _identifier.MaybeVisualize(graph, n, ctx);
            _openingBracket.MaybeVisualize(graph, n, ctx);
            foreach (var a in _attributes ?? [])
                a.Visualize(graph, n, ctx);
            _closingBracket.MaybeVisualize(graph, n, ctx);
            return n;
        }
#endif
    }
}