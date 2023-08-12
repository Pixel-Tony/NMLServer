using NMLServer.Lexing;
using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer;

internal class Program
{
    public static void Main(string[] args)
    {
        string[] tests =
        {
            "1 ? 2 : 3 + 1 ? 4 : 5"
        };
        var tokensArrays = tests.Select(test => new Lexer(test).Tokenize().ToArray());
        var parsers = tokensArrays.Select(tokens => new ExpressionParser(tokens));
        var list = parsers.Select(parser => parser.Parse(0));
        foreach (var (a, b) in list)
        {
            Console.WriteLine(a?.ToString() ?? (a is null).ToString());
            Console.WriteLine((b as LiteralToken)?.value ?? null);
        }
    }
}