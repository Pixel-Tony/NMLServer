namespace NMLServer.Model.Grammar;

/// <summary>
/// The enumeration of symbol types for <see cref="Lexis.IdentifierToken"/> token.
/// </summary>
[Flags]
internal enum SymbolKind
{
    None = 0,

#pragma warning disable format
    Feature                 = 0x1,
    Function                = 0x2,
    Variable                = 0x3,
    Parameter               = 0x4,
    Constant                = 0x5,
    Property                = 0x6,

    KindMask                = 0xF,

    ReadableOutsideSwitch   = 0x40,
    Writeable               = 0x80,
#pragma warning restore format
}