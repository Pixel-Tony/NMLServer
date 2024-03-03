using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal sealed partial class TownNames
{
    private readonly partial struct Part : IHasEnd
    {
        private readonly BracketToken _openingBracket;
        private readonly IReadOnlyList<TownNamesPartTextEntry>? _texts;
        private readonly IReadOnlyList<SubEntry>? _subParts;
        private readonly BracketToken? _closingBracket;

        public int end => _closingBracket?.end ?? Extensions.LastOf(_texts, _subParts) switch
        {
            0 => _openingBracket.end,
            var v => v
        };

        public Part(ParsingState state, BracketToken openingBracket)
        {
            _openingBracket = openingBracket;
            List<TownNamesPartTextEntry> texts = [];
            List<SubEntry> subParts = [];
            for (var token = state.nextToken; token is not null;)
            {
                switch (token)
                {
                    case BracketToken { Bracket: '}' } closingBracket:
                        _closingBracket = closingBracket;
                        state.Increment();
                        goto label_End;

                    case IdentifierToken:
                        var call = ExpressionAST.TryParse(state, true)!;
                        token = state.currentToken;
                        if (token is BinaryOpToken { Type: OperatorType.Comma } commaInText)
                        {
                            texts.Add(new TownNamesPartTextEntry(call, commaInText));
                            break;
                        }
                        texts.Add(new TownNamesPartTextEntry(call));
                        continue;

                    case KeywordToken { Type: KeywordType.TownNames } subPartKeyword:
                        state.IncrementSkippingComments();
                        var args = ExpressionAST.TryParse(state, true);

                        token = state.currentToken;
                        if (token is BinaryOpToken { Type: OperatorType.Comma } commaInSubPart)
                        {
                            subParts.Add(new SubEntry(subPartKeyword, args, commaInSubPart));
                            break;
                        }
                        subParts.Add(new SubEntry(subPartKeyword, args));
                        continue;

                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                        goto label_End;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
                token = state.nextToken;
            }
            label_End:
            _texts = texts.ToMaybeList();
            _subParts = subParts.ToMaybeList();
        }
    }
}