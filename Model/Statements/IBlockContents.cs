using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements;

internal interface IBlockContents<T> : IHasEnd, IVisualProvider where T : IBlockContents<T>
{
    public static abstract List<T>? ParseSomeInBlock(ref ParsingState state, ref BracketToken? closingBracket);
}