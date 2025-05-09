namespace NMLServer.Model;

internal enum KeywordKind
{
    None = 0,
    ReturnKeyword = 1,
    ExpressionUsable = 2,
    BlockDefining = 4,
    CallDefining = 8
}