using NMLServer.Model.Statements;

namespace NMLServer.Model.Processors;

internal interface IIncrementalNodeProcessor
{
    public void Trim((int begin, int end) range, Range protocolRange);

    public void Process(BaseStatement node, NodeProcessingContext context);

    public void FinishIncrement() { }
}
