using NMLServer.Parsing;

namespace NMLServer.Lexing.Tokens;

internal class BinaryOpToken : Token, IHasPrecedence
{
    public readonly string Operation;
    public int precedence => Grammar.OperatorPrecedence[Operation];
    public BinaryOpToken(string operation) => Operation = operation;

    public static bool operator >(BinaryOpToken left, IHasPrecedence right) => left.precedence > right.precedence;
    public static bool operator <(BinaryOpToken left, IHasPrecedence right) => left.precedence < right.precedence;
}