namespace NMLServer.Model.Lexis;

internal abstract class BaseMulticharToken(int start, int end) : Token(start, end - start);