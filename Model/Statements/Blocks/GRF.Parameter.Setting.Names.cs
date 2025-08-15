using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Extensions;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal partial class GRF
{
    private partial class Parameter
    {
        public partial struct Setting
        {
            private readonly struct Names : IHasEnd, IFoldingRangeProvider
            {
                private readonly IdentifierToken? _name;
                private readonly ColonToken? _colon;
                private readonly BracketToken _openingBracket;
                private readonly List<PropertySetting>? _items;
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
                    _items = PropertySetting.ParseSomeInBlock(ref state, ref _closingBracket);
                    if (_closingBracket is not null)
                        _semicolon = state.ExpectSemicolon();
                }

                public void ProvideFoldingRanges(List<FoldingRange> ranges, ref PositionConverter converter)
                    => IFoldingRangeProvider.RangeFromBrackets(_openingBracket, _closingBracket, ranges, ref converter);
            }
        }
    }
}