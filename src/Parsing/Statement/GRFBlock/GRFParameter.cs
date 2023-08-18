using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

/*
 * param:
 * param [<num>] {
 *   <name> {
 *     type:       <"int"|"bool">;
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
    public NumericToken? ParameterNumber;
    public BracketToken? OpeningBracket;
    public IdentifierToken? Name;
    public BracketToken? InnerOpeningBracket;
    public NMLAttribute[]? Attributes;
    public ParameterTypeAttribute[]? ParameterType;
    public NamesAttribute[]? Names;
    public BracketToken? InnerClosingBracket;
    public BracketToken? ClosingBracket;

    public GRFParameter(GRFBlock grfBlock, KeywordToken alwaysPARAM) : base(grfBlock, alwaysPARAM)
    { }

    public override string ToString()
    {
        return $"  {Type.value} {ParameterNumber?.value ?? ""} {OpeningBracket?.Bracket ?? '.'}\n"
               + $"    {Name?.value ?? "."} {InnerOpeningBracket?.Bracket ?? '.'}\n"
               + $"      {(Attributes != null ? string.Join("\n", Attributes) : "")}\n"
               + $"      {(Names != null ? string.Join("\n", Names) : "")}\n"
               + $"    {InnerClosingBracket?.Bracket ?? '.'}\n"
               + $"  {ClosingBracket?.Bracket ?? '.'}";
    }
}