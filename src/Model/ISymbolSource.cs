using NMLServer.Model.Lexis;

namespace NMLServer.Model;

/// <summary>
/// The interface for describing top-level structures capable of defining new symbols: switches, spritesets, items, etc.
/// </summary>
// TODO: implement support for all statements that define symbols
internal interface ISymbolSource
{
    public IdentifierToken? Symbol { get; }
}