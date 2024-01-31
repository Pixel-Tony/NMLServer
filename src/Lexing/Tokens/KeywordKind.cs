namespace NMLServer;

internal enum KeywordKind : byte
{
    None = 0,
    ReturnKeyword = 0b0001,
    ExpressionUsable = 0b0010,
    BlockDefining = 0b0100,
    FunctionBlockDefining = 0b1000
}