using NMLServer.Extensions;
using NMLServer.Model.Processors.Diagnostics;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal sealed class Cargotable(ref ParsingState state, KeywordToken keyword)
    : BaseBlockStatement<ValueWithComma<BaseValueToken>>(ref state, keyword)
{
    public override void ProvideDiagnostics(DiagnosticContext context)
    {
        base.ProvideDiagnostics(context);
        if (Contents is null)
            return;
        foreach (var item in Contents.ToReadOnlySpan()[..^1])
        {
            if (item.Comma is null)
                context.Add(ErrorStrings.Err_MissingComma, item.Identifier.End);
            if (item.Identifier is not StringToken)
                context.Add(ErrorStrings.Err_ExpectedString, item.Identifier);
        }
        if (Contents[^1].Identifier is { } id and not StringToken)
            context.Add(ErrorStrings.Err_ExpectedString, id);
    }
}