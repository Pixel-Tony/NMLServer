using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

internal class StatementParser : BaseParser
{
    /// <summary>Try to parse singular top-level statement from given input and starting point.</summary>
    /// <param name="parent">A parent instance to set as a parent for resulting statement instance.</param>
    /// <param name="start">A starting index of token stream.</param>
    /// <returns></returns>
    public static (BaseStatementAST?, Token?) ParseTopLevelStatement(NMLFileRoot parent, int start)
    {
        if (start >= Max)
        {
            return (null, null);
        }
        Pointer = start;

        var startingToken = Tokens[Pointer];
        if (startingToken is not KeywordToken keywordToken)
        {
            return (null, null);
        }
        return keywordToken.value switch
        {
            "grf" => GRFBlockParser.ParseGRFBlock(parent, keywordToken),
            _ => (null, startingToken)
        };
    }
}