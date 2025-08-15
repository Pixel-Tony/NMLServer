using NMLServer.Model.Syntax;
using NMLServer.Model.Statements;

namespace NMLServer.Model.Processors;

internal interface IIncrementalNodeProcessor
{
    public void Trim();

    public void Process(BaseStatement node, NodeProcessingContext context);

    public void FinishIncrement(ref readonly AbstractSyntaxTree ast);
}