using NMLServer.Parsing.Expression;

namespace NMLServer.Lexing.Tokens;

internal class TernaryOpToken : Token, IHasPrecedence
{
    public int precedence => TernaryOperation.Precedence;
}