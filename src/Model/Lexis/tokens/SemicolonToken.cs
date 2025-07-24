namespace NMLServer.Model.Lexis;

internal sealed class SemicolonToken(int start) : Token(start, 1)
{
    public override string? SemanticType => null;
}