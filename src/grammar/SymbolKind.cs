namespace NMLServer;

internal enum SymbolKind : byte
{
    None = 0,
    Feature,
    Function,
    Macro,
    Variable,
    Parameter,
    Constant
}