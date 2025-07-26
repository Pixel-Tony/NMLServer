using NMLServer.Model.Lexis;

namespace NMLServer.Model.Statement;

internal interface IBlockContents<T> : IHasEnd, IVisualProvider where T : IBlockContents<T>
{
    public static abstract List<T>? ParseSomeInBlock(ref ParsingState state, ref BracketToken? closingBracket);
}