using System.Runtime.CompilerServices;
using NMLServer.Lexing;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace NMLServer;

internal static class Extensions
{
    public static List<T>? ToMaybeList<T>(this List<T> target)
    {
        return target.Count > 0
            ? target
            : null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LastOf<T1, T2>(IReadOnlyList<T1>? first, IReadOnlyList<T2>? second)
        where T1 : IHasEnd where T2 : IHasEnd
        => int.Max(first?[^1].end ?? 0, second?[^1].end ?? 0);

    public static SemanticTokenType? ToGeneralTokenType(this SymbolKind target)
    {
        return (target & (SymbolKind)0x0F) switch
        {
            SymbolKind.Feature => SemanticTokenType.Type,
            SymbolKind.Switch => SemanticTokenType.Function,
            SymbolKind.Macro => SemanticTokenType.Macro,
            SymbolKind.Variable => SemanticTokenType.Variable,
            SymbolKind.Parameter => SemanticTokenType.Parameter,
            SymbolKind.Constant => SemanticTokenType.EnumMember,
            _ => null as SemanticTokenType?
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetLength(this Token token)
    {
        return token switch
        {
            BaseMulticharToken multicharToken => multicharToken.Length,
            UnitToken unitToken => unitToken.length,
            RangeToken => 2,
            _ => 1
        };
    }

    // public static void VerifyAsParameter(this ExpressionAST target, DiagnosticsContext diagnosticsContext, EvaluatedExpressionType expectedType)
    // {
    //     if (target.evaluatedType != expectedType)
    //     {
    //         // TODO;
    //         return;
    //     }
    // }

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