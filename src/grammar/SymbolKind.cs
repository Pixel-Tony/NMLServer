namespace NMLServer;

/// <summary>
/// The enumeration of symbol types for <see cref="NMLServer.Lexing.IdentifierToken"/> token.
/// </summary>
[Flags]
internal enum SymbolKind : byte
{
    None = 0,
    Feature = 0x01,
    Switch = 0x02,
    Macro = 0x04,
    Variable = 0x05,
    Parameter = 0x06,
    Constant = 0x07,

    UserDefined = 0x20,
    ReadableOutsideSwitch = 0x40,
    Writeable = 0x80
}