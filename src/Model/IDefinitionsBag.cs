using System.Diagnostics.CodeAnalysis;
using NMLServer.Model.Lexis;

namespace NMLServer.Model;

internal interface IDefinitionsBag
{
    public bool Has(IdentifierToken token, [NotNullWhen(true)] out List<IdentifierToken>? definitions);
}