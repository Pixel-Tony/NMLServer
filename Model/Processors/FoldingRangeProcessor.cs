using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Extensions;
using NMLServer.Model.Statements;

namespace NMLServer.Model.Processors;

internal sealed class FoldingRangeProcessor : IIncrementalNodeProcessor
{
    public readonly List<FoldingRange> Ranges = [];
    private readonly List<FoldingRange> _newRanges = [];
    private int beforeIndex = -1;
    private int afterIndex = -1;

    public void Trim((int, int) _, Range protocolRange)
    {
        var (start, end) = protocolRange;
        beforeIndex = -1;
        afterIndex = Ranges.Count;
        for (int left = 0, right = afterIndex - 1; left <= right;)
        {
            var mid = left + (right - left) / 2;
            var item = Ranges[mid];
            if (item.EndLine >= start.Line | (item.EndLine == start.Line & item.EndCharacter > start.Character))
            {
                right = mid - 1;
                continue;
            }
            beforeIndex = mid;
            left = mid + 1;
        }
        beforeIndex = int.Max(beforeIndex, 0);
        for (int left = beforeIndex, right = afterIndex - 1; left <= right;)
        {
            var mid = left + (right - left) / 2;
            var item = Ranges[mid];
            if (item.StartLine <= end.Line | (item.StartLine == end.Line & item.StartCharacter < end.Character))
            {
                left = mid + 1;
                continue;
            }
            afterIndex = mid;
            right = mid - 1;
        }
    }

    public void Process(BaseStatement node, NodeProcessingContext ctx) => node.ProvideFoldingRanges(in _newRanges, in ctx.Converter);

    public void FinishIncrement()
    {
        Ranges.ReplaceRange((beforeIndex, afterIndex), in _newRanges);
        _newRanges.Clear();
        beforeIndex = afterIndex = -1;
    }
}
