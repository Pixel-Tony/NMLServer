using LanguageServer.Client;
using LanguageServer.Parameters.TextDocument;
using LanguageServer.Parameters.Window;
using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing;

internal abstract class BaseParser
{
    protected static Token[] Tokens = null!;
    protected static int Pointer;
    protected static int Max;
    protected static List<Token> UnexpectedTokens = null!;

    protected static bool areTokensLeft => Pointer < Max;

    public static void GetUnexpectedTokensDiagnostics(List<Diagnostic> l, IPositionConverter converter, Proxy a)
    {
        a.Window.LogMessage(new LogMessageParams
        {
            message = $"found {UnexpectedTokens.Count} unexpected tokens",
            type = UnexpectedTokens.Count == 0
                ? MessageType.Log
                : MessageType.Warning
        });

        int i = 0;
        foreach (var unexpectedToken in UnexpectedTokens)
        {
            if (++i > 400)
            {
                return;
            }

            l.Add(new Diagnostic
            {
                severity = DiagnosticSeverity.Error,
                message = "Unexpected token",
                range = new Range
                {
                    start = converter[unexpectedToken.Start],
                    end = converter[unexpectedToken is BaseMulticharToken hasEnd
                        ? hasEnd.End
                        : unexpectedToken.Start + 1]
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