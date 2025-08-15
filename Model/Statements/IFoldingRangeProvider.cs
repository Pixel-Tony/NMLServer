using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements;

interface IFoldingRangeProvider
{
    public void ProvideFoldingRanges(List<FoldingRange> ranges, ref PositionConverter converter);

    public sealed static void RangeWithInnerRanges<T>(BracketToken? start, BracketToken? end, IEnumerable<T>? contents,
        List<FoldingRange> ranges, ref PositionConverter converter) where T : IFoldingRangeProvider
    {
        if (start is null | end is null)
        {
            Include(contents, ranges, ref converter);
            return;
        }
        var startPos = converter.LocalToProtocol(start!.Start);
        Include(contents, ranges, ref converter);
        var endPos = converter.LocalToProtocol(end!.Start);
        ranges.Add(FromDocumentRange(startPos, endPos));
    }

    public sealed static void RangeFromBrackets(BracketToken? start, BracketToken? end,
        List<FoldingRange> ranges, ref PositionConverter converter, bool copy = false)
            => RangeFromOffsets(start?.Start, end?.End, ranges, ref converter, copy);

    public sealed static void RangeFromOffsets(int? start, int? end, List<FoldingRange> ranges,
        ref PositionConverter converter, bool copy = false)
    {
        if (start is null | end is null)
            return;
        var startPos = converter.LocalToProtocol(start!.Value);
        var endPos = copy
            ? converter.Copy().LocalToProtocol(end!.Value)
            : converter.LocalToProtocol(end!.Value);
        ranges.Add(FromDocumentRange(startPos, endPos));
    }

    public sealed static void Include<T>(IEnumerable<T>? contents, List<FoldingRange> ranges,
        ref PositionConverter converter) where T : IFoldingRangeProvider
    {
        if (contents is not null)
            foreach (var item in contents)
                item.ProvideFoldingRanges(ranges, ref converter);
    }

    private static FoldingRange FromDocumentRange(Position start, Position end)
        => new()
        {
            Kind = FoldingRangeKind.Region,
            StartLine = (uint)start.Line,
            StartCharacter = (uint)start.Character,
            EndLine = (uint)end.Line,
            EndCharacter = (uint)end.Character,
        };
}