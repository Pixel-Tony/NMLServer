using NMLServer.Extensions;
using NMLServer.Model.Lexis;
using NMLServer.Model.Expression;
using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;

namespace NMLServer.Model.Statement;

internal sealed partial class TownNames
{
    private readonly partial struct Part : IHasEnd, IFoldingRangeProvider
    {
        private readonly BracketToken _openingBracket;
        private readonly List<TextEntry>? _texts;
        private readonly List<SubEntry>? _subParts;
        private readonly BracketToken? _closingBracket;

        public int End => _closingBracket?.End
                          ?? (IHasEnd.LastOf(_texts, _subParts, out int v) ? v : _openingBracket.End);

        public Part(ref ParsingState state, BracketToken openingBracket)
        {
            _openingBracket = openingBracket;
            state.Increment();
            List<TextEntry> texts = [];
            List<SubEntry> subParts = [];
            while (state.CurrentToken is { } token)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '}' } closingBracket:
                        _closingBracket = closingBracket;
                        state.Increment();
                        goto label_End;

                    case IdentifierToken:
                        var call = ExpressionAST.TryParse(ref state, true)!;
                        token = state.CurrentToken;
                        if (token is BinaryOpToken { Type: OperatorType.Comma } commaInText)
                        {
                            texts.Add(new TextEntry(call, commaInText));
                            state.Increment();
                            break;
                        }
                        texts.Add(new TextEntry(call));
                        break;

                    case KeywordToken { Type: KeywordType.TownNames } townNames:
                        state.IncrementSkippingComments();
                        var args = ExpressionAST.TryParse(ref state, true);

                        token = state.CurrentToken;
                        if (token is BinaryOpToken { Type: OperatorType.Comma } comma)
                        {
                            subParts.Add(new SubEntry(townNames, args, comma));
                            state.Increment();
                            break;
                        }
                        subParts.Add(new SubEntry(townNames, args));
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
            _texts = texts.ToMaybeList();
            _subParts = subParts.ToMaybeList();
        }

        public void ProvideFoldingRanges(in Stack<IFoldingRangeProvider> children, in List<FoldingRange> ranges,
            ref TokenStorage.PositionConverter converter)
        {
            IFoldingRangeProvider.RangeFromBrackets(_openingBracket, _closingBracket, in ranges, ref converter);
        }
    }
}