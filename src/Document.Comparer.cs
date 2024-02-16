using NMLServer.Lexing;

namespace NMLServer;

internal partial class Document
{
    private class IdentifierComparer : IIdentifierTokenComparer
    {
        public string Context;

        public IdentifierComparer(string context)
        {
            Context = context;
        }

        public bool Equals(IdentifierToken? x, IdentifierToken? y)
        {
            return ReferenceEquals(x, y) || (x is not null && y is not null && x.Equals(y, Context));
        }

        public int GetHashCode(IdentifierToken obj) => obj.Hash;
    }
}