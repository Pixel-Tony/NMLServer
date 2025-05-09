using NMLServer.Extensions;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal readonly struct ValueWithComma<T>(T identifier, BinaryOpToken? comma)
    : IAllowsParseInsideBlock<ValueWithComma<T>> where T : BaseValueToken
{
    public int end => comma?.end ?? identifier.end;

    public static List<ValueWithComma<T>>? ParseSomeInBlock(ref ParsingState state, ref BracketToken? closingBracket)
    {
        List<ValueWithComma<T>> chain = [];
        T? current = null;
        for (var token = state.currentToken; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case BinaryOpToken { Type: OperatorType.Comma } commaToken when current is not null:
                    chain.Add(new ValueWithComma<T>(current, commaToken));
                    current = null;
                    break;

                case T value when current is null:
                    current = value;
                    break;

                case BracketToken { Bracket: '}' } expectedClosingBracket:
                    closingBracket = expectedClosingBracket;
                    state.Increment();
                    goto label_End;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.CallDefining }:
                    goto label_End;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
        label_End:
        return chain.ToMaybeList();
    }
}