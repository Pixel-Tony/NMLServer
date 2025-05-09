using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal interface IAllowsParseInsideBlock<T> : IHasEnd where T : IAllowsParseInsideBlock<T>
{
    public static abstract List<T>? ParseSomeInBlock(ref ParsingState state, ref BracketToken? closingBracket);
}