using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Extensions;
using NMLServer.Model.Expressions;
using NMLServer.Model.Processors.Diagnostics;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal sealed partial class TownNames : BaseBlockStatement
{
    private readonly List<PropertySetting>? _attributes;
    private readonly List<Part>? _parts;

    protected override int? MiddleEnd => (_attributes, _parts).LastOf();

    public TownNames(ref ParsingState state, KeywordToken keyword) : base(ref state, keyword)
    {
        if (ClosingBracket is not null)
            return;

        List<PropertySetting> attributes = [];
        List<Part> parts = [];
        while (state.CurrentToken is { } token)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' } openingInnerBracket:
                    parts.Add(new Part(ref state, openingInnerBracket));
                    break;

                case BracketToken { Bracket: '}' } closingBracket:
                    ClosingBracket = closingBracket;
                    state.Increment();
                    goto label_End;

                case IdentifierToken identifier:
                    attributes.Add(new PropertySetting(ref state, identifier));
                    break;

                case KeywordToken { IsDefiningStatement: true }:
                    goto label_End;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
    label_End:
        _attributes = attributes.ToMaybeList();
        _parts = parts.ToMaybeList();
    }

    protected override IdentifierToken? CaptureSymbol() => ((Arguments as ParentedExpression)?.Expression as Identifier)?.Token;

    public override void ProvideDiagnostics(DiagnosticContext context)
    {
        if (Arguments is not null)
            base.ProvideDiagnostics(context);
    }

    public override void ProvideFoldingRanges(List<FoldingRange> ranges, ref PositionConverter converter)
        => IFoldingRangeProvider.RangeWithInnerRanges(OpeningBracket, ClosingBracket, _parts, ranges, ref converter);
}