using NMLServer.Lexing;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal sealed partial class TownNames
{
    private readonly partial struct Part : IHasEnd
    {
        private readonly BracketToken _openingBracket;
        private readonly List<TextEntry>? _texts;
        private readonly List<SubEntry>? _subParts;
        private readonly BracketToken? _closingBracket;

        public int end
        {
            get
            {
                if (_closingBracket is not null)
                {
                    return _closingBracket.end;
                }
                for (var last = Extensions.LastOf(_texts, _subParts); last > 0;)
                {
                    return last;
                }
                return _openingBracket.end;
            }
        }

        public Part(ref ParsingState state, BracketToken openingBracket)
        {
            _openingBracket = openingBracket;
            List<TextEntry> texts = [];
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
                        var call = ExpressionAST.TryParse(ref state, true)!;
                        token = state.currentToken;
                        if (token is BinaryOpToken { Type: OperatorType.Comma } commaInText)
                        {
                            texts.Add(new TextEntry(call, commaInText));
                            break;
                        }
                        texts.Add(new TextEntry(call));
                        continue;

                    case KeywordToken { Type: KeywordType.TownNames } townNames:
                        state.IncrementSkippingComments();
                        var args = ExpressionAST.TryParse(ref state, true);

                        token = state.currentToken;
                        if (token is BinaryOpToken { Type: OperatorType.Comma } comma)
                        {
                            subParts.Add(new SubEntry(townNames, args, comma));
                            break;
                        }
                        subParts.Add(new SubEntry(townNames, args));
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