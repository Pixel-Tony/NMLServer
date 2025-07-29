using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal partial class GRFBlock
{
    private partial class Parameter
    {
        public partial struct Block
        {
            private readonly struct Names : IHasEnd, IFoldingRangeProvider
            {
                private readonly IdentifierToken? _name;
                private readonly ColonToken? _colon;
                private readonly BracketToken _openingBracket;
                private readonly List<NMLAttribute>? _items;
                private readonly BracketToken? _closingBracket;
                private readonly SemicolonToken? _semicolon;

                public int End => _semicolon?.End ?? _closingBracket?.End
                    ?? _items?[^1].End ?? _openingBracket?.End ?? _colon?.End ?? _name!.End;

                public Names(ref ParsingState state, IdentifierToken? name, ColonToken? colon,
                    BracketToken openingBracket)
                {
                    _name = name;
                    _colon = colon;
                    _openingBracket = openingBracket;
                    state.Increment();
                    _items = NMLAttribute.ParseSomeInBlock(ref state, ref _closingBracket);
                    if (_closingBracket is not null)
                        _semicolon = state.ExpectSemicolon();
                }

                public void ProvideFoldingRanges(in Stack<IFoldingRangeProvider> children, in List<FoldingRange> ranges,
                    ref TokenStorage.PositionConverter converter)
                {
                    IFoldingRangeProvider.RangeFromBrackets(_openingBracket, _closingBracket, in ranges, ref converter);
                }
            }
        }
    }
}