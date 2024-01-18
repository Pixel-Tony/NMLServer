using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statement;

internal class GRFParameter : BaseParametrizedStatementWithBlock
{
    private readonly IdentifierToken? _name;
    private readonly BracketToken? _innerOpeningBracket;
    private readonly NMLAttribute[]? _attributes;
    private readonly GRFParameterNames[]? _names;
    private readonly BracketToken? _innerClosingBracket;

    public GRFParameter(ParsingState state, KeywordToken keyword) : base(state, keyword)
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

                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                    return;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }

        List<NMLAttribute> attributes = new();
        List<GRFParameterNames> names = new();

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

                        case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                            goto ending;

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

                        case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                            goto ending;

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
                        case KeywordToken { IsExpressionUsable: true }:
                        case TernaryOpToken:
                        case UnaryOpToken:
                        case ValueToken:
                        case BinaryOpToken:
                            var expr = ExpressionAST.TryParse(state);
                            var semicolon = state.ExpectSemicolonAfterExpression();
                            attributes.Add(new NMLAttribute(identifier, colon, expr, semicolon));
                            identifier = null;
                            colon = null;
                            innerState = InnerState.ExpectingIdentifier;
                            token = state.currentToken;
                            continue;

                        case BracketToken { Bracket: '{' } openingNamesBracket:
                            names.Add(new GRFParameterNames(state, identifier, colon, openingNamesBracket));
                            identifier = null;
                            colon = null;
                            innerState = InnerState.ExpectingIdentifier;
                            token = state.currentToken;
                            continue;

                        case BracketToken { Bracket: '}' } innerClosingBracket:
                            _innerClosingBracket = innerClosingBracket;
                            break;

                        case KeywordToken { Type: not KeywordType.Return }:
                            goto ending;

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

                default:
                    throw new ArgumentOutOfRangeException(nameof(innerState), "Unexpected state");
            }
            token = state.nextToken;
        }
        if (_innerClosingBracket is not null)
        {
            ClosingBracket = state.ExpectClosingCurlyBracket();
        }

        ending:
        _attributes = attributes.ToMaybeArray();
        _names = names.ToMaybeArray();
    }

    private enum InnerState
    {
        ExpectingIdentifier,
        ExpectingColon,
        ExpectingBody
    }
}