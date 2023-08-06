using NMLServer.Parsing;

namespace NMLServer.Lexing.Tokens;

internal class BinaryOpToken : Token
{
    public readonly string Operation;
    
    public int precedence => Grammar.OperatorPrecedence[Operation];
    public BinaryOpToken(string operation) => Operation = operation;
}