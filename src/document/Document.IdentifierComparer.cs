using NMLServer.Lexing;

namespace NMLServer;

internal partial class Document
{
    private class IdentifierComparer(string context) : IIdentifierTokenComparer
    {
        public string context { get; set; } = context;

        public bool Equals(IdentifierToken? x, IdentifierToken? y)
            => ReferenceEquals(x, y)
               || (x is not null && y is not null && x.ContextuallyEqual(y, context));

        public int GetHashCode(IdentifierToken obj) => obj.Hash;
    }
}