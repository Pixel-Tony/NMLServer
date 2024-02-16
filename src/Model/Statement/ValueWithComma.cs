using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal readonly record struct ValueWithComma<T>(T Identifier, BinaryOpToken? Comma) : IAllowsParseInsideBlock<ValueWithComma<T>>
    where T : BaseValueToken
{
    public readonly T Identifier = Identifier;
    public readonly BinaryOpToken? Comma = Comma;

    public int end => Comma?.end ?? Identifier.end;

    public static List<ValueWithComma<T>>? ParseSomeInBlock(ParsingState state, ref BracketToken? closingBracket)
    {
        List<ValueWithComma<T>> chain = new();
        T? current = null;
        for (var token = state.currentToken; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case BinaryOpToken { Type: OperatorType.Comma } comma when current is not null:
                    chain.Add(new(current, comma));
                    current = null;
                    break;

                case T value when current is null:
                    current = value;
                    break;

                case BracketToken { Bracket: '}' } expectedClosingBracket:
                    closingBracket = expectedClosingBracket;
                    state.Increment();
                    goto label_End;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.FunctionBlockDefining }:
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