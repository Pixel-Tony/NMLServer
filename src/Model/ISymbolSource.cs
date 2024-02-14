using NMLServer.Lexing;

namespace NMLServer.Model;

/// <summary>
/// The interface describing top-level structures like switches or parameter assignments,
/// capable of defining new symbols.
/// </summary>
internal interface ISymbolSource
{
    public IdentifierToken? symbol { get; }
}