using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Statement;

/* grf:
 * grf {
 *   grfid: <literal-string>;
 * 	 name: <string>;
 * 	 desc: <string>;
 * 	 version: <expression>;
 * 	 min_compatible_version: <expression>;
 * 	 [url: <string>;]
 * 	 [<nml:param>*]
 *  }
 *
 */
internal class GRFBlock : BaseTitledDeclaration
{
    public BracketToken? OpeningBracket;
    public NMLAttribute[]? Attributes;
    public GRFParameter[]? Parameters;
    public BracketToken? ClosingBracket;

    public GRFBlock(NMLFileRoot? parent, KeywordToken alwaysGRF) : base(parent, alwaysGRF)
    { }

    public override string ToString()
    {
        return $"{Type.value} {OpeningBracket?.Bracket ?? '.'}\n"
               + $"  {(Attributes != null ? string.Join("\n", Attributes) : "")}\n"
               + $"  {(Parameters != null ? string.Join("\n", Parameters as IEnumerable<GRFParameter>) : "")}";
    }
}