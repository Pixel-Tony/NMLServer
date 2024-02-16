using NMLServer.Lexing;

namespace NMLServer.Model.Statement;

internal interface IAllowsParseInsideBlock<T> : IHasEnd where T : IAllowsParseInsideBlock<T>
{
    public static abstract List<T>? ParseSomeInBlock(ParsingState state, ref BracketToken? closingBracket);
}