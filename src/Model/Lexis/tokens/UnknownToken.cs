namespace NMLServer.Model.Lexis;

internal sealed class UnknownToken(int start) : Token(start, 1)
{
    public override string? SemanticType => null;
}