using DotNetGraph.Core;
using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using NMLServer.Extensions;
using NMLServer.Model.Expressions;
using NMLServer.Model.Grammar;
using NMLServer.Model.Processors.Diagnostics;
using NMLServer.Model.Tokens;

using static NMLServer.Model.Processors.Diagnostics.ErrorStrings;

namespace NMLServer.Model.Statements;

internal abstract class BaseStatement : IHasBounds, IVisualProvider
{
    public abstract int Start { get; }

    public abstract int End { get; }

    public virtual DotNode Visualize(DotGraph graph, DotNode parent, StringView ctx)
        => VizExtensions.MakeNode(graph, parent, label: "Stmt").WithStmtFeatures();

    public virtual void AddDefinitions(DefinitionBag map, StringView source)
    {
    }

    public virtual void ProvideFoldingRanges(List<FoldingRange> ranges, ref PositionConverter converter)
    {
    }

    public virtual void ProvideDiagnostics(DiagnosticContext context)
    {
    }

    protected static IdentifierToken? CaptureSymbol(KeywordToken keyword, BaseExpression? arguments)
    {
        if (!keyword.RequiresParens)
            return (arguments as Identifier)?.Token;
        if (arguments is not ParentedExpression { Expression: { } expr })
            return null;

        var symbolIndex = keyword.SymbolIndex;
        for (var argCount = 1; ; ++argCount)
        {
            if (expr is not BinaryOperation { OperatorType: OperatorType.Comma } binary)
                return symbolIndex == argCount - 1
                    ? (expr as Identifier)?.Token
                    : null;

            if (symbolIndex == argCount - 1)
                return (binary.Left as Identifier)?.Token;
            expr = binary.Right;
        }
    }

    protected static void ProcessFunctionSyntax(DiagnosticContext context, KeywordToken keyword,
        BaseExpression? arguments, bool isIdentifierRequired)
    {
        ParentedExpression? args = null;
        switch (arguments)
        {
            case null:
                context.Add(isIdentifierRequired
                    ? Err_UniqueIdentifierArgsExpected
                    : Err_ExpectedBlockArguments,
                    keyword.End);
                return;

            case FunctionCall call:
                args = call.Arguments;
                if (isIdentifierRequired)
                    if (call.Function is not IdentifierToken { Kind: SymbolKind.None })
                        context.Add(Err_UniqueIdentifierExpected, call.Function);
                else
                    context.Add(Err_UnexpectedToken, call.Function);
                break;

            case ParentedExpression expr:
                if (isIdentifierRequired)
                    context.Add(Err_UniqueIdentifierExpected, expr.Start - 1);
                args = expr;
                break;
        }
        ProcessArgumentList(context, keyword, args);
    }

    protected static void ProcessArgumentList(DiagnosticContext context, KeywordToken keyword,
        BaseExpression? arguments)
    {
        var fallback = keyword.End;
        var minCount = keyword.MinParams;
        if (!keyword.RequiresParens)
        {
            if (minCount > 0)
            {
                if (arguments is null)
                    context.Add(Err_ExpectedBlockArguments, fallback);
            }
            else if (arguments is not null)
                context.Add(Err_UnexpectedBlockArguments, arguments);
            return;
        }

        var symbolIndex = keyword.SymbolIndex;
        BaseExpression? Symbol = null;
        IHasEnd? BeforeSymbol = null;
        IHasStart? AfterSymbol = null;

        var maxCount = keyword.MaxParams;
        BaseExpression? _excess = null;
        BinaryOpToken? prevComma = null;
        var _excessArgs = false;

        if (arguments is not ParentedExpression
            {
                OpeningBracket: var openingParen,
                Expression: var inner,
                ClosingBracket: var closingParen
            })
        {
            var (start, end) = arguments is null
                ? (fallback, fallback)
                : (arguments.Start, arguments.End);
            context.Add(Err_ArgumentListInParensExpected, start, end);
            return;
        }
        if (openingParen is null)
            context.Add(Err_MissingLeftParen, (inner?.Start - 1) ?? fallback);
        else
        {
            BeforeSymbol = openingParen;
            if (openingParen.Bracket is not '(')
                context.Add(Err_IncorrectParenTypeLeftParenExpected, openingParen.Start);
        }

        if (closingParen is null)
            context.Add(Err_MissingRightParen, inner?.End ?? openingParen?.End ?? fallback);
        else
        {
            AfterSymbol = closingParen;
            if (closingParen.Bracket is not ')')
                context.Add(Err_IncorrectParenTypeRightParenExpected, closingParen.Start);
        }

        if (inner is not { } expr)
        {
            if (minCount > 0)
                context.Add(Err_ExpectedArguments, arguments);
            return;
        }
        uint argCount = 0;
        while (true)
        {
            ++argCount;
            if (argCount > maxCount & !_excessArgs)
            {
                _excessArgs = true;
                _excess = expr;
            }
            if (expr is not BinaryOperation { OperatorType: OperatorType.Comma } binary)
            {
                if (symbolIndex == argCount - 1)
                    Symbol = expr;
                break;
            }
            expr = binary.Right;
            prevComma = binary.Operator;

            if (argCount >= 2 && symbolIndex <= argCount - 2)
                BeforeSymbol = binary.Operator;
            else if (symbolIndex == argCount - 1)
            {
                Symbol = binary.Left;
                AfterSymbol = binary.Operator;
            }
        }

        if (argCount < minCount)
        {
            if (arguments is null)
                context.Add(minCount > 1 ? Err_BlockExpectsArguments : Err_BlockExpectsArgument, keyword);
            else
                context.Add(Err_NotEnoughBlockArgs, arguments);
        }

        if (_excessArgs)
        {
            var start = _excess?.Start ?? prevComma?.Start ?? arguments!.Start;
            var end = _excess?.End ?? arguments!.End;
            context.Add(Err_ExcessBlockArgs, start, end);
        }

        if ((AfterSymbol is not null)
            & (argCount >= uint.Max(minCount, uint.Min(symbolIndex, uint.MaxValue - 1) + 1))
            & (Symbol is not Identifier { Kind: SymbolKind.None })
            )
        {
            var start = Symbol?.Start ?? BeforeSymbol?.End ?? fallback;
            var end = Symbol?.End ?? AfterSymbol!.Start;
            context.Add(Err_ExpectedBlockIdentifier, start, end);
        }

        // public void VerifyContext(ref readonly DiagnosticContext context, DefinitionBag definitions)
        // {
        //     if (Symbol is null || !definitions.Has(Symbol, out var others))
        //         return;

        //     if (others.IndexOf(Symbol) != 0)
        //         context.Add("Identifier is already defined", Symbol);
        // }
    }
}