using NMLServer.Lexing;

namespace NMLServer;

internal partial class Document
{
    private class IdentifierComparer : IIdentifierTokenComparer
    {
        private readonly string _context;

        public IdentifierComparer(string context)
        {
            _context = context;
        }

        public bool Equals(IdentifierToken? x, IdentifierToken? y)
        {
            return ReferenceEquals(x, y) || (x is not null && y is not null && x.Equals(y, _context));
        }

        public int GetHashCode(IdentifierToken obj) => obj.Hash;
    }
}