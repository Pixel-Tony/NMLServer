namespace NMLServer.Model.Tokens;

internal sealed class UnknownToken(int start) : BaseToken(start, 1)
{
    public override string? SemanticType => null;
}