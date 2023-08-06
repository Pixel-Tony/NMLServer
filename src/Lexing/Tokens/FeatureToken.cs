namespace NMLServer.Lexing.Tokens;

internal class FeatureToken : Token
{
    public readonly string Feature;

    public FeatureToken(string type)
    {
        Feature = type;
    }
}