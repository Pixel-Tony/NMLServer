using LanguageServer.Parameters.TextDocument;
using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing;

internal abstract class BaseParser
{
    protected static Token[] Tokens = null!;
    protected static int Pointer;
    protected static int Max;
    protected static List<Token> UnexpectedTokens = null!;

    public static void GetUnexpectedTokensDiagnostics(List<Diagnostic> l, IPositionConverter converter)
    {
        foreach (var unexpectedToken in UnexpectedTokens)
        {
            l.Add(new Diagnostic
            {
                severity = DiagnosticSeverity.Error,
                message = "Unexpected token",
                range = new Range
                {
                    start = converter[unexpectedToken.Start],
                    end = converter[unexpectedToken.Start + 1],
                }
            });
        }
    }

    public static void Use(Token[] tokens)
    {
        Tokens = tokens;
        Max = Tokens.Length;
        Pointer = 0;
        UnexpectedTokens = new List<Token>();
    }
}