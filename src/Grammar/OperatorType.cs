namespace NMLServer;

internal enum OperatorType : byte
{
    Comma,
    QuestionMark,
    Colon,
    LogicalOr,
    LogicalAnd,
    BinaryOr,
    BinaryXor,
    BinaryAnd,
    Eq,
    Ne,
    Le,
    Ge,
    Lt,
    Gt,
    ShiftLeft,
    ShiftRight,
    ShiftRightFunky,
    Plus,
    Minus,
    Multiply,
    Divide,
    Modula,
    LogicalNot,
    BinaryNot,
}