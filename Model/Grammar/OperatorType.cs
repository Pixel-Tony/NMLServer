namespace NMLServer.Model.Grammar;

internal enum OperatorType
{
    None = 0,
#pragma warning disable format
    Comma           = 0x01,
    QuestionMark    = 0x10,
    Colon           = 0x11,
    LogicalOr       = 0x20,
    LogicalAnd      = 0x30,
    BinaryOr        = 0x40,
    BinaryXor       = 0x50,
    BinaryAnd       = 0x60,
    Eq              = 0x70,
    Ne              = 0x71,
    Le              = 0x72,
    Ge              = 0x73,
    Lt              = 0x74,
    Gt              = 0x75,
    ShiftLeft       = 0x80,
    ShiftRight      = 0x81,
    ShiftRightFunky = 0x82,
    Plus            = 0x90,
    Minus           = 0x91,
    Multiply        = 0xA0,
    Divide          = 0xA1,
    Modula          = 0xA2,
    LogicalNot      = 0xB0,
    BinaryNot       = 0xB1,
#pragma warning restore format
}