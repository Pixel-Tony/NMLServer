using NMLServer.Model.Expressions;
using NMLServer.Model.Processors.Diagnostics;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Statements.Blocks;

internal sealed class Template(ref ParsingState state, KeywordToken keyword)
    : BaseSpriteHolder(ref state, keyword)
{
    protected override IdentifierToken? CaptureSymbol() => Arguments switch
    {
        FunctionCall call => call.Function as IdentifierToken,
        Identifier id => id.Token,
        _ => null
    };

    protected override void ProcessArguments(DiagnosticContext context)
        => ProcessFunctionSyntax(context, Keyword, Arguments, true);
}