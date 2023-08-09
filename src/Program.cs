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
        const string test = "a + b * c + d";
        
        var tokens = new Lexer(test).Tokenize().ToArray();
        var parser = new ExpressionParser();
        var (a, b) = parser.Parse(tokens, 0);

        Console.WriteLine(a?.ToString() ?? (a is null).ToString());
        Console.WriteLine(b is null);
    }
}
