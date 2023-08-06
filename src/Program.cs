// using System.Text;
// using NMLServer.Parsing.Lexing;
// using NMLServer.Parsing;

using NMLServer.Lexing;
using NMLServer.Parsing.Expression;

namespace NMLServer;

internal class Program
{
    public static void Main(string[] args)
    {
        const string test = "a + b * c + d / e + f + g";
        using var enumerator = new Lexer(test).Tokenize().GetEnumerator();
        var (a, b) = new OldExpressionParser(enumerator).ParseExpression();
        Console.WriteLine(a?.ToString() ?? (a is null).ToString());
        Console.WriteLine(b is null);
    }
}
