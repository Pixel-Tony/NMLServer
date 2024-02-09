using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer;

internal static class Extentions
{
    public static List<T>? ToMaybeList<T>(this List<T> target) => target.Count > 0
        ? target
        : null;

    public static SemanticTokenType ToGeneralTokenType(this SymbolKind target) => target switch
    {
        SymbolKind.Feature => SemanticTokenType.Type,
        SymbolKind.Function => SemanticTokenType.Function,
        SymbolKind.Macro => SemanticTokenType.Macro,
        SymbolKind.Variable => SemanticTokenType.Variable,
        SymbolKind.Parameter => SemanticTokenType.Parameter,
        SymbolKind.Constant => SemanticTokenType.EnumMember,
        _ => SemanticTokenType.Variable
    };

    // public static void VerifyAsParameter(this ExpressionAST target, DiagnosticsContext diagnosticsContext, EvaluatedExpressionType expectedType)
    // {
    //     if (target.evaluatedType != expectedType)
    //     {
    //         // TODO;
    //         return;
    //     }
    // }
    //
    // public static void VerifyAsParameters(this ExpressionAST target, DiagnosticsContext diagnosticsContext,
    //     params EvaluatedExpressionType[] paramTypes)
    // {
    //     if (target is not ParentedExpression { Expression: var args })
    //     {
    //         // TODO: expected parens
    //         return;
    //     }
    //     for (int i = 0; i < paramTypes.Length - 1; ++i)
    //     {
    //         if (args is null)
    //         {
    //             // TODO: (..., <null here> )
    //             return;
    //         }
    //         if (args is not BinaryOperation { Operation.Type: OperatorType.Comma } commaJoinedArguments)
    //         {
    //             // TODO: expected x params, got x - n (n > 0)
    //             return;
    //         }
    //         args = commaJoinedArguments.Right;
    //         var left = commaJoinedArguments.left;
    //         if (left is null)
    //         {
    //             // TODO: missing parameter
    //         }
    //         else if (left.evaluatedType != paramTypes[i])
    //         {
    //             // TODO: type mismatch
    //         }
    //     }
    //     if (args is BinaryOperation { Operation.Type: OperatorType.Comma } excessCommaJoinedArguments)
    //     {
    //         // TODO: expected x params, got x + n (n > 0)
    //     }
    //     else if (args is null)
    //     {
    //         // TODO: (..., <null here> )
    //     }
    //     else if (args.evaluatedType != paramTypes[^1])
    //     {
    //         // TODO: type mismatch
    //     }
    // }
}