using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal partial class GRFBlock
{
    private sealed partial class Parameter : BaseStatementWithBlock
    {
        private readonly IdentifierToken? _name;
        private readonly BracketToken? _innerOpeningBracket;
        private readonly IReadOnlyList<NMLAttribute>? _attributes;
        private readonly IReadOnlyList<Names>? _names;
        private readonly BracketToken? _innerClosingBracket;

        public Parameter(ParsingState state, KeywordToken keyword) : base(state, keyword)
        {
            if (ClosingBracket is not null)
            {
                return;
            }

            bool inside = false;
            for (var token = state.currentToken; !inside && token is not null; token = state.nextToken)
            {
                switch (token)
                {
                    case IdentifierToken identifierToken when _name is null:
                        _name = identifierToken;
                        break;

                    case BracketToken { Bracket: '}' } closingBracket:
                        ClosingBracket = closingBracket;
                        state.Increment();
                        return;

                    case BracketToken { Bracket: '{' } innerOpeningBracket:
                        _innerOpeningBracket = innerOpeningBracket;
                        inside = true;
                        break;

                    case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                        return;

                    default:
                        state.AddUnexpected(token);
                        break;
                }
            }

            List<NMLAttribute> attributes = new();
            List<Names> names = new();

            IdentifierToken? identifier = null;
            ColonToken? colon = null;
            InnerState innerState = InnerState.ExpectingIdentifier;

            for (var token = state.currentToken; _innerClosingBracket is null && token is not null;)
            {
                switch (innerState)
                {
                    case InnerState.ExpectingIdentifier:
                        switch (token)
                        {
                            case BracketToken { Bracket: '}' } closingBracket:
                                _innerClosingBracket = closingBracket;
                                break;

                            case ColonToken colonToken:
                                colon = colonToken;
                                innerState = InnerState.ExpectingBody;
                                break;

                            case IdentifierToken identifierToken:
                                identifier = identifierToken;
                                innerState = InnerState.ExpectingColon;
                                break;

                            case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                                goto label_End;

                            default:
                                state.AddUnexpected(token);
                                break;
                        }
                        break;

                    case InnerState.ExpectingColon:
                        switch (token)
                        {
                            case BracketToken { Bracket: '}' } closingBracket:
                                _innerClosingBracket = closingBracket;
                                break;

                            case ColonToken colonToken:
                                colon = colonToken;
                                innerState = InnerState.ExpectingBody;
                                break;

                            case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
                                goto label_End;

                            case SemicolonToken semicolonToken:
                                attributes.Add(new NMLAttribute(identifier, null, null, semicolonToken));
                                identifier = null;
                                innerState = InnerState.ExpectingIdentifier;
                                break;

                            default:
                                state.AddUnexpected(token);
                                break;
                        }
                        break;

                    case InnerState.ExpectingBody:
                        switch (token)
                        {
                            case UnitToken:
                            case KeywordToken { Kind: KeywordKind.ExpressionUsable }:
                            case TernaryOpToken:
                            case UnaryOpToken:
                            case BaseValueToken:
                            case BinaryOpToken:
                                var expr = ExpressionAST.TryParse(state);
                                var semicolon = state.ExpectSemicolon();
                                token = state.currentToken;
                                attributes.Add(new NMLAttribute(identifier, colon, expr, semicolon));
                                identifier = null;
                                colon = null;
                                innerState = InnerState.ExpectingIdentifier;
                                continue;

                            case BracketToken { Bracket: '{' } openingNamesBracket:
                                names.Add(new Names(state, identifier, colon, openingNamesBracket));
                                identifier = null;
                                colon = null;
                                innerState = InnerState.ExpectingIdentifier;
                                token = state.currentToken;
                                continue;

                            case BracketToken { Bracket: '}' } innerClosingBracket:
                                _innerClosingBracket = innerClosingBracket;
                                break;

                            case KeywordToken { Type: not KeywordType.Return }:
                                goto label_End;

                            case SemicolonToken semicolonToken:
                                attributes.Add(new NMLAttribute(identifier, colon, null, semicolonToken));
                                identifier = null;
                                colon = null;
                                innerState = InnerState.ExpectingIdentifier;
                                break;

                            default:
                                state.AddUnexpected(token);
                                break;
                        }
                        break;
                }
                token = state.nextToken;
            }
            if (_innerClosingBracket is not null)
            {
                ClosingBracket = state.ExpectClosingCurlyBracket();
            }
            label_End:
            _attributes = attributes.ToMaybeList();
            _names = names.ToMaybeList();
        }

        private enum InnerState
        {
            ExpectingIdentifier,
            ExpectingColon,
            ExpectingBody
        }
    }
}