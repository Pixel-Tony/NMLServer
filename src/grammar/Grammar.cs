using System.Text;
using NMLServer.Lexing;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using static NMLServer.OperatorType;

namespace NMLServer;

internal static class Grammar
{
    public const int TernaryOperatorPrecedence = 1;

    public static readonly Dictionary<OperatorType, byte> OperatorPrecedences = new()
    {
        [Comma] = 0,
        [QuestionMark] = TernaryOperatorPrecedence,
        [Colon] = TernaryOperatorPrecedence,
        [LogicalOr] = 2,
        [LogicalAnd] = 3,
        [BinaryOr] = 4,
        [BinaryXor] = 5,
        [BinaryAnd] = 6,
        [Eq] = 7,
        [Ne] = 7,
        [Le] = 7,
        [Ge] = 7,
        [Lt] = 7,
        [Gt] = 7,
        [ShiftLeft] = 8,
        [ShiftRight] = 8,
        [ShiftRightFunky] = 8,
        [Plus] = 9,
        [Minus] = 9,
        [Multiply] = 10,
        [Divide] = 10,
        [Modula] = 10,
        [LogicalNot] = 11,
        [BinaryNot] = 11
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
                _definedSymbols[symbol] = kind;
            }
        }
        catch (Exception e)
        {
            Program.LogInfo(e.ToString());
        }
    }

    public static SymbolKind GetSymbolKind(ReadOnlySpan<char> needle)
    {
        _definedSymbols.TryGetValue(new string(needle), out var result);
        return result;
    }
}