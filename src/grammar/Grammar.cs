using System.Text;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;

namespace NMLServer;

internal static class Grammar
{
    public const int TernaryOperatorPrecedence = 1;

    public static readonly Dictionary<OperatorType, byte> OperatorPrecedences = new()
    {
        [OperatorType.Comma] = 0,
        [OperatorType.QuestionMark] = TernaryOperatorPrecedence,
        [OperatorType.Colon] = TernaryOperatorPrecedence,
        [OperatorType.LogicalOr] = 2,
        [OperatorType.LogicalAnd] = 3,
        [OperatorType.BinaryOr] = 4,
        [OperatorType.BinaryXor] = 5,
        [OperatorType.BinaryAnd] = 6,
        [OperatorType.Eq] = 7,
        [OperatorType.Ne] = 7,
        [OperatorType.Le] = 7,
        [OperatorType.Ge] = 7,
        [OperatorType.Lt] = 7,
        [OperatorType.Gt] = 7,
        [OperatorType.ShiftLeft] = 8,
        [OperatorType.ShiftRight] = 8,
        [OperatorType.ShiftRightFunky] = 8,
        [OperatorType.Plus] = 9,
        [OperatorType.Minus] = 9,
        [OperatorType.Multiply] = 10,
        [OperatorType.Divide] = 10,
        [OperatorType.Modula] = 10,
        [OperatorType.LogicalNot] = 11,
        [OperatorType.BinaryNot] = 11
    };

    public static OperatorType GetOperatorType(ReadOnlySpan<char> needle)
        => needle switch
        {
            "," => OperatorType.Comma,
            "?" => OperatorType.QuestionMark,
            ":" => OperatorType.Colon,
            "||" => OperatorType.LogicalOr,
            "&&" => OperatorType.LogicalAnd,
            "|" => OperatorType.BinaryOr,
            "^" => OperatorType.BinaryXor,
            "&" => OperatorType.BinaryAnd,
            "==" => OperatorType.Eq,
            "!=" => OperatorType.Ne,
            "<=" => OperatorType.Le,
            ">=" => OperatorType.Ge,
            "<" => OperatorType.Lt,
            ">" => OperatorType.Gt,
            "<<" => OperatorType.ShiftLeft,
            ">>" => OperatorType.ShiftRight,
            ">>>" => OperatorType.ShiftRightFunky,
            "+" => OperatorType.Plus,
            "-" => OperatorType.Minus,
            "*" => OperatorType.Multiply,
            "/" => OperatorType.Divide,
            "%" => OperatorType.Modula,
            "!" => OperatorType.LogicalNot,
            "~" => OperatorType.BinaryNot,
            _ => OperatorType.None
        };

    public static OperatorType GetOperatorType(char needle) => GetOperatorType(stackalloc char[] { needle });

    private static readonly Dictionary<string, SymbolKind> _definedSymbols = new();

    static Grammar()
    {
        AddFromFile("constants.txt", SymbolKind.Constant);
        AddFromFile("functions.txt", SymbolKind.Function);
        AddFromFile("variables.txt", SymbolKind.Variable);
        AddFromFile("features.txt", SymbolKind.Feature);
    }

    private static void AddFromFile(string filename, SymbolKind kind)
    {
        try
        {
            filename = Path.Combine(AppContext.BaseDirectory, "grammar", filename);

            foreach (var symbol in File.ReadAllText(filename, Encoding.UTF8).Split(' '))
            {
                _definedSymbols.TryAdd(symbol, kind);
            }
        }
        catch (Exception e)
        {
            Program.Server?.LogInfo(e.ToString());
        }
    }

    public static SymbolKind GetSymbolKind(ReadOnlySpan<char> needle)
    {
        _definedSymbols.TryGetValue(new string(needle), out var result);
        return result;
    }
}