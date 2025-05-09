using NMLServer.Model.Lexis;

namespace NMLServer.Model;

internal partial class Document
{
    private class IdentifierComparer : IEqualityComparer<IdentifierToken>
    {
        public string context { get; set; } = null!;

        public bool Equals(IdentifierToken? x, IdentifierToken? y)
            => ReferenceEquals(x, y)
               || (x is not null && y is not null && x.IsContextuallyEqual(y, context));

        public int GetHashCode(IdentifierToken obj) => obj.Hash;
    }
}