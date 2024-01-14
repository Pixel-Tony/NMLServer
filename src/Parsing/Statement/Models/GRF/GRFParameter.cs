using NMLServer.Lexing.Tokens;
using NMLServer.Parsing.Expression;

namespace NMLServer.Parsing.Statements.Models;

/*
 * param:
 * param [<num>] {
 *   <name> {
 *     type:       int | bool;
 *     name:       <expression>;
 *     desc:       <expression>;
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
internal record struct GRFParameter(KeywordToken AlwaysParam)
{
    public KeywordToken AlwaysParam = AlwaysParam;
    public ExpressionAST? ParameterNumber;
    public BracketToken? OpeningBracket;
    public IdentifierToken? Name;
    public BracketToken? InnerOpeningBracket;
    public NMLAttribute[]? Attributes;
    public NamesAttribute[]? Names;
    public BracketToken? InnerClosingBracket;
    public BracketToken? ClosingBracket;
}