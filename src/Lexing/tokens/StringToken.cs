namespace NMLServer.Lexing;

internal sealed class StringToken(int start, int end) : BaseValueToken(start, end);