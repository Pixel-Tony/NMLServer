namespace NMLServer.Model.Tokens;

internal abstract class BaseMulticharToken(int start, int end) : BaseToken(start, end - start);