namespace NMLServer.Lexing;

internal sealed class UnaryOpToken(int start, char sign) : Token(start);