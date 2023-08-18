using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing;

internal record struct Pair<TKey, TValue>(TKey? Key = null, ColonToken? Colon = null, TValue? Value = default,
    SemicolonToken? Semicolon = null)
    where TKey : Token
{
    public readonly TKey? Key = Key;
    public ColonToken? Colon = Colon;
    public TValue? Value = Value;
    public SemicolonToken? Semicolon = Semicolon;

    public override string ToString()
    {
        return $"({Key?.ToString() ?? "."}{(Colon != null ? ":" : "")}"
               + $" {Value?.ToString() ?? "."}{(Semicolon != null ? ";" : ".")})";
    }
}