using System.Runtime.CompilerServices;
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


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OperatorType GetOperatorType(StringView needle)
        => needle switch
        {
            "," => Comma,
            "?" => QuestionMark,
            ":" => Colon,
            "||" => LogicalOr,
            "&&" => LogicalAnd,
            "|" => BinaryOr,
            "^" => BinaryXor,
            "&" => BinaryAnd,
            "==" => Eq,
            "!=" => Ne,
            "<=" => Le,
            ">=" => Ge,
            "<" => Lt,
            ">" => Gt,
            "<<" => ShiftLeft,
            ">>" => ShiftRight,
            ">>>" => ShiftRightFunky,
            "+" => Plus,
            "-" => Minus,
            "*" => Multiply,
            "/" => Divide,
            "%" => Modula,
            "!" => LogicalNot,
            "~" => BinaryNot,
            _ => None
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OperatorType GetOperatorType(char needle)
        => needle switch
        {
            ',' => Comma,
            '?' => QuestionMark,
            ':' => Colon,
            '|' => BinaryOr,
            '^' => BinaryXor,
            '&' => BinaryAnd,
            '<' => Lt,
            '>' => Gt,
            '+' => Plus,
            '-' => Minus,
            '*' => Multiply,
            '/' => Divide,
            '%' => Modula,
            '!' => LogicalNot,
            '~' => BinaryNot,
            _ => None
        };

    private static readonly Dictionary<string, SymbolKind> _definedSymbols = new();

    static Grammar()
    {
        AddFromFile("constants.txt", SymbolKind.Constant);
        AddFromFile("misc-bits.txt", SymbolKind.Variable | SymbolKind.Writeable);
        AddFromFile("readable.txt", SymbolKind.Variable);
        AddFromFile("functions.txt", SymbolKind.Switch);
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

    public static SymbolKind GetSymbolKind(StringView needle)
    {
        _definedSymbols.TryGetValue(new string(needle), out var result);
        return result;
    }

    // TODO: nullable readonly struct, why?
    public static bool GetTokenSemanticType(Token token, out SemanticTokenType? type)
    {
        return (type = token switch
        {
            CommentToken => SemanticTokenType.Comment,
            IdentifierToken id => id.kind.ToSemanticType(),
            UnitToken
                or KeywordToken => SemanticTokenType.Keyword,
            NumericToken => SemanticTokenType.Number,
            StringToken => SemanticTokenType.String,
            ColonToken
                or RangeToken
                or BracketToken
                or UnaryOpToken
                or BinaryOpToken
                or TernaryOpToken
                or SemicolonToken
                or AssignmentToken
                => SemanticTokenType.Operator,
            _ => null
        }) is not null;
    }
}