namespace NMLServer.Lexing;

internal sealed class IdentifierToken : BaseValueToken
{
    public SymbolKind kind { get; }

    public IdentifierToken(int start, int end, SymbolKind symbolKind) : base(start, end)
    {
        kind = symbolKind;
    }
}