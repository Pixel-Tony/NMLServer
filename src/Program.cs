using NMLServer.Lexing;
using NMLServer.Parsing;
using NMLServer.Parsing.Expression;
using NMLServer.Parsing.Statement;

namespace NMLServer;

internal class Program
{
    public static void Main(string[] args)
    {
        string grfBlockTest = ReadFile();
        var (tokens, _) = new Lexer(grfBlockTest).Tokenize();
        BaseParser.Use(tokens);
        Console.WriteLine(tokens.Length);
        
        var parentFile = new NMLFileRoot();
        var (a, b) = StatementParser.ParseTopLevelStatement(parentFile, 0);
        Console.WriteLine(a?.ToString() ?? null);
        Console.WriteLine(b?.ToString() ?? null);
    }

    private static string ReadFile()
    {
        using FileStream fileStream = new FileStream("test.nml", FileMode.Open);
        using StreamReader reader = new StreamReader(fileStream);
        return reader.ReadToEnd();
    }
}