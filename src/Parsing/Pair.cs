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

    public static Pair<TKey, TValue> From<TOther>(Pair<TKey, TOther> sketch, TValue value)
        => new(sketch.Key, sketch.Colon, value);

}