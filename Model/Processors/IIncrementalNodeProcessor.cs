using NMLServer.Model.Syntax;
using NMLServer.Model.Statements;

namespace NMLServer.Model.Processors;

internal interface IIncrementalNodeProcessor
{
    public void ProcessChangedSyntax(ref TreeTraverser traverser, BaseStatement? end, ref readonly IncrementContext context);
}