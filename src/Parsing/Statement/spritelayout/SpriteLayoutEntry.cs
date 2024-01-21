using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal readonly struct SpriteLayoutEntry
{
    public readonly IdentifierToken? Identifier;
    public readonly BracketToken? OpeningBracket;
    public readonly NMLAttribute[]? Attributes;
    private readonly BracketToken? _closingBracket;

    public SpriteLayoutEntry(ParsingState state, BracketToken openingBracket)
    {
        OpeningBracket = openingBracket;
        state.Increment();
        Attributes = NMLAttribute.TryParseManyInBlock(state, ref _closingBracket);
    }

    public SpriteLayoutEntry(ParsingState state, IdentifierToken identifier)
    {
        Identifier = identifier;
        for (var token = state.nextToken; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' } openingBracket:
                    OpeningBracket = openingBracket;
                    state.Increment();
                    Attributes = NMLAttribute.TryParseManyInBlock(state, ref _closingBracket);
                    return;

                case BracketToken { Bracket: '}' } closingBracket:
                    _closingBracket = closingBracket;
                    state.Increment();
                    return;

                case KeywordToken { IsExpressionUsable: false, Type: not KeywordType.Return }:
                    return;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
    }
}