namespace NMLServer.Model.Tokens;

internal sealed class SemicolonToken(int start) : BaseToken(start, 1)
{
    public override string? SemanticType => null;
}