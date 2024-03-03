namespace NMLServer.Lexing;

internal sealed class NumericToken(int start, int end) : BaseValueToken(start, end);