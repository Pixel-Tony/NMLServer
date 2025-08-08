using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements;

interface IFoldingRangeProvider
{
    public void ProvideFoldingRanges(in List<FoldingRange> ranges, ref readonly PositionConverter converter);

    public sealed static void RangeWithInnerRanges<T>(BracketToken? start, BracketToken? end, IEnumerable<T>? contents,
        in List<FoldingRange> ranges, ref readonly PositionConverter converter) where T : IFoldingRangeProvider
    {
        if (start is null | end is null)
        {
            Include(contents, in ranges, in converter);
            return;
        }
        var startPos = converter.LocalToProtocol(start!.Start);
        Include(contents, in ranges, in converter);
        var endPos = converter.LocalToProtocol(end!.Start);
        ranges.Add(FromDocumentRange(startPos, endPos));
    }

    public sealed static void RangeFromBrackets(BracketToken? start, BracketToken? end,
        in List<FoldingRange> ranges, ref readonly PositionConverter converter, bool copy = false)
            => RangeFromOffsets(start?.Start, end?.End, in ranges, in converter, copy);

    public sealed static void RangeFromOffsets(int? start, int? end, in List<FoldingRange> ranges,
        ref readonly PositionConverter converter, bool copy = false)
    {
        if (start is null | end is null)
            return;
        var startPos = converter.LocalToProtocol(start!.Value);
        var endPos = copy
            ? converter.Copy().LocalToProtocol(end!.Value)
            : converter.LocalToProtocol(end!.Value);
        ranges.Add(FromDocumentRange(startPos, endPos));
    }

    public sealed static void Include<T>(IEnumerable<T>? contents, in List<FoldingRange> ranges,
        ref readonly PositionConverter converter) where T : IFoldingRangeProvider
    {
        if (contents is not null)
            foreach (var item in contents)
                item.ProvideFoldingRanges(in ranges, in converter);
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