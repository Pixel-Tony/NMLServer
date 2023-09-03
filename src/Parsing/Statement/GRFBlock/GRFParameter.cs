using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;
using NMLServer.Parsing.Statement.Results;

namespace NMLServer.Parsing.Statement;

/*
 * param:
 * param [<num>] {
 *   <name> {
 *     type:       int | bool;
 *     name:       <string>;
 *     desc:       <string>;
 *     min_value:  <expression>;
 *     max_value:  <expression>;
 *     def_value:  <expression>;
 *     bit:        <expression>;
 *     names: {
 *       [<num>: <string>;]
 *     };
 *   }
 * }
 */
internal class GRFParameter : BaseTitledStatement
{
    private NumericToken? ParameterNumber;
    private BracketToken? OpeningBracket;
    private IdentifierToken? Name;
    private BracketToken? InnerOpeningBracket;
    private NMLAttribute[]? Attributes;
    private NamesAttribute[]? Names;
    private BracketToken? InnerClosingBracket;
    private BracketToken? ClosingBracket;

    private GRFParameter(GRFBlock grfBlock, KeywordToken alwaysParam) : base(grfBlock, alwaysParam)
    { }

    public static GRFParameter? FromParseResult(in GRFBlock parent, BlockStatementParseResult data, string textualContext)
    {
        if (data.Keyword is not KeywordToken paramKeyword || paramKeyword.Value(textualContext) != "param")
        {
            return null;
        }

        if (data.Body.Blocks.Count < 1)
        {
            return new GRFParameter(parent, paramKeyword)
            {
                ParameterNumber = null,
                OpeningBracket = data.Body.OpeningBracket,
                ClosingBracket = data.Body.ClosingBracket
            };
        }

        var inside = data.Body.Blocks[0];
        
        // TODO

        return new GRFParameter(parent, paramKeyword)
        {
            ParameterNumber = (data.Parameters as Number)?.token as NumericToken,
            OpeningBracket = data.Body.OpeningBracket,
            Name = inside.Keyword as IdentifierToken,
            InnerOpeningBracket = inside.Body.OpeningBracket,
            Attributes = inside.Body.Attributes.ToArray(),
            Names = inside.Body.NamesBlocks.ToArray(),
            InnerClosingBracket = inside.Body.ClosingBracket,
            ClosingBracket = data.Body.ClosingBracket
        };
    }
}