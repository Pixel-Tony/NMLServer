namespace NMLServer.Model;

/// <summary>
/// The enumeration of symbol types for <see cref="Lexis.IdentifierToken"/> token.
/// </summary>
[Flags]
internal enum SymbolKind
{
    Undefined = 0,
    Feature = 0x01,
    Function = 0x02,
    Variable = 0x03,
    Parameter = 0x04,
    Constant = 0x05,

    KindMask = 0x0F,

    ReadableOutsideSwitch = 0x40,
    Writeable = 0x80,
}