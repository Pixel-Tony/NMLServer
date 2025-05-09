namespace NMLServer.Model.Lexis;

internal sealed class UnknownToken(int start) : Token(start, 1)
{
    internal override string? semanticType => null;
}