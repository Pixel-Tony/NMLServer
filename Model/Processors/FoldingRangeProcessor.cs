using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Model.Syntax;
using NMLServer.Model.Statements;

namespace NMLServer.Model.Processors;

internal sealed class FoldingRangeProcessor : IIncrementalNodeProcessor
{
    public readonly List<FoldingRange> Content = [];
    // private readonly List<FoldingRange> _newRanges = [];
    // private int _beforeIndex = -1;
    // private int _afterIndex = -1;

    public void Trim()
    {
        // var (start, end) = protocolRange;

        // _beforeIndex = -1;
        // _afterIndex = Ranges.Count;
        // for (int left = 0, right = _afterIndex - 1; left <= right;)
        // {
        //     var mid = left + (right - left) / 2;
        //     var item = Ranges[mid];
        //     if (item.EndLine > start.Line || (item.EndLine == start.Line && item.EndCharacter > start.Character))
        //     {
        //         right = mid - 1;
        //         continue;
        //     }
        //     _beforeIndex = mid;
        //     left = mid + 1;
        // }
        // _beforeIndex = int.Max(_beforeIndex, 0);
        // for (int left = _beforeIndex, right = _afterIndex - 1; left <= right;)
        // {
        //     var mid = left + (right - left) / 2;
        //     var item = Ranges[mid];
        //     if (item.StartLine < end.Line || (item.StartLine == end.Line && item.StartCharacter < end.Character))
        //     {
        //         left = mid + 1;
        //         continue;
        //     }
        //     _afterIndex = mid;
        //     right = mid - 1;
        // }
    }

    public void Process(BaseStatement node, NodeProcessingContext ctx)
    {
        // node.ProvideFoldingRanges(in _newRanges, ref ctx.Converter);
    }

    public void FinishIncrement(ref readonly AbstractSyntaxTree tree)
    {
        // Logger.Debug($"Removed {_afterIndex -_beforeIndex}, added {_newRanges.Count} range(s)");
        // Ranges.ReplaceRange((_beforeIndex, _afterIndex), _newRanges);
        // _newRanges.Clear();
        // _beforeIndex = _afterIndex = -1;
        Content.Clear();
        PositionConverter converter = tree.Tokens.MakeConverter();
        for (TreeTraverser trv = new(tree); trv.Current is { } node; trv.Increment())
            node.ProvideFoldingRanges(Content, ref converter);
    }
}
