using NMLServer.Model;
using NMLServer.Model.Tokens;

namespace NMLServer.Extensions;

internal static class ParsingStateExtensions
{
    public static SemicolonToken? ExpectSemicolon(this ref ParsingState state)
    {
        while (state.CurrentToken is { } token)
        {
            switch (token)
            {
                case SemicolonToken semicolon:
                    state.Increment();
                    return semicolon;

                case BracketToken { Bracket: '}' }:
                case KeywordToken { IsDefiningStatement: true }:
                    return null;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        return null;
    }
}