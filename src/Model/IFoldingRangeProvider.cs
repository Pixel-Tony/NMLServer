using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

interface IFoldingRangeProvider
{
    public void ProvideFoldingRanges(in Stack<IFoldingRangeProvider> children,
        in List<FoldingRange> ranges, ref TokenStorage.PositionConverter converter);

    public sealed static void RangeFromBrackets(BracketToken? start, BracketToken? end,
        in List<FoldingRange> ranges, ref TokenStorage.PositionConverter converter, bool copy = false)
            => RangeFromOffsets(start?.Start, end?.End, in ranges, ref converter, copy);

    public sealed static void RangeFromOffsets(int? start, int? end, in List<FoldingRange> ranges,
        ref TokenStorage.PositionConverter converter, bool copy = false)
    {
        if (start is null | end is null)
            return;
        var startPos = converter.LocalToProtocol(start!.Value);
        var endPos = (copy ? converter.Copy() : converter).LocalToProtocol(end!.Value);
        ranges.Add(new FoldingRange()
        {
            Kind = FoldingRangeKind.Region,
            StartLine = (uint)startPos.Line,
            StartCharacter = (uint)startPos.Character,
            EndLine = (uint)endPos.Line,
            EndCharacter = (uint)endPos.Character,
        });
    }

    public sealed static void Include<T>(IEnumerable<T>? contents, in Stack<IFoldingRangeProvider> children)
        where T : IFoldingRangeProvider
    {
        if (contents is not null)
            foreach (var item in contents)
                children.Push(item);
    }
}