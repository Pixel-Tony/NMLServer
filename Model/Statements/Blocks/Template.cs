using NMLServer.Model.Expressions;
using NMLServer.Model.Grammar;
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
    {
        ParentedExpression? args = null;
        switch (Arguments)
        {
            case null:
                context.Add(ErrorStrings.Err_UniqueIdentifierArgsExpected, Keyword.End);
                return;

            case FunctionCall call:
                if (call.Function is var token and not IdentifierToken { Kind: SymbolKind.None })
                    context.Add(ErrorStrings.Err_UniqueIdentifierExpected, token);
                args = call.Arguments;
                break;

            case ParentedExpression expr:
                context.Add(ErrorStrings.Err_UniqueIdentifierExpected, expr.Start - 1);
                args = expr;
                break;
        }
        ProcessArgumentList(context, Keyword, Keyword.End, args);
    }
}