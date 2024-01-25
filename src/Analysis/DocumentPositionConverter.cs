using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace NMLServer.Analysis;

internal readonly record struct DocumentPositionConverter
{
    private readonly string[] _context;

    public DocumentPositionConverter(string context)
    {
        _context = context.Split('\n');
    }

    public IEnumerable<Range> this[Position start, Position end]
    {
        get
        {
            if (start.Line == end.Line)
            {
                yield return new Range(start, end);
                yield break;
            }
            yield return new Range(start, new Position(start.Line, _context[start.Line].Length - 1));

            int endLine = end.Line;
            for (var line = start.Line + 1; line < endLine; ++line)
            {
                yield return new Range(line, 0, line, _context[line].Length - 1);
            }
            yield return new Range(new Position(endLine, 0), end);
        }
    }

    public Position this[int start]
    {
        get
        {
            int line = 0;
            int length = _context[line].Length + 1;
            while (start >= length)
            {
                start -= length;
                length = _context[++line].Length + 1;
            }
            return new Position(line, start);
        }
    }
}