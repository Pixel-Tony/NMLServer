using NMLServer.Lexing;
using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer;

internal class Program
{
    public static void Main(string[] args)
    {
        const string test = "!3 + !7 * !!~~!5 - ~6";
        var tokens = new Lexer(test).Tokenize().ToArray();
        var parser = new ExpressionParser(tokens);
        var (a, b) = parser.Parse(0);
        Console.WriteLine(a?.ToString() ?? (a is null).ToString());
        Console.WriteLine((b as LiteralToken)?.value ?? null);
    }
}
