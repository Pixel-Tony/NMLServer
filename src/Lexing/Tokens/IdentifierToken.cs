namespace NMLServer.Lexing.Tokens;

internal sealed class IdentifierToken : BaseValueToken
{
    public SymbolKind semanticType { get; }

    public IdentifierToken(int start, int end, SymbolKind type) : base(start, end)
    {
        semanticType = type;
    }
}