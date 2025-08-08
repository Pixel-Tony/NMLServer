using NMLServer.Model.Diagnostics;
using NMLServer.Model.Tokens;
using NMLServer.Model.Expressions;
using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Extensions;

namespace NMLServer.Model.Statements.Blocks;

internal abstract class BaseBlockStatement : BaseStatement
{
    private readonly KeywordToken _keyword;
    protected readonly BaseExpression? Parameters;
    public readonly BracketToken? OpeningBracket;
    public BracketToken? ClosingBracket;

    public sealed override int Start => _keyword.Start;

    public sealed override int End => ClosingBracket?.End ?? MiddleEnd ?? OpeningBracket?.End ?? Parameters?.End ?? _keyword.End;

    protected abstract int? MiddleEnd { get; }

    protected BaseBlockStatement(ref ParsingState state, KeywordToken keyword)
    {
        _keyword = keyword;
        state.Increment();
        Parameters = BaseExpression.TryParse(ref state);
        while (state.CurrentToken is { } token)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' } openingBracket:
                    OpeningBracket = openingBracket;
                    state.Increment();
                    return;

                case BracketToken { Bracket: '}' } closingBracket:
                    ClosingBracket = closingBracket;
                    state.Increment();
                    return;

                case KeywordToken { IsDefiningStatement: true }:
                    return;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
        return;

        // TODO remove!
        // IdentifierToken? CaptureSymbol()
        // {
        //     if (ParameterInfo.Symbol.kind is SymbolKind.Undefined)
        //         return null;
        //     if (Parameters is not ParentedExpression { Expression: var expr })
        //         return null;
        //     var parameters = UnpackParams(expr);
        //     if (ParameterInfo.Symbol.index > parameters.Count - 1)
        //         return null;

        //     if (parameters[^(ParameterInfo.Symbol.index + 1)]
        //         is not Identifier { Kind: SymbolKind.Undefined, Token: var token })
        //         return null;
        //     token.Kind = ParameterInfo.Symbol.kind;
        //     return token;
        // }
    }

    public virtual void VerifySyntax(ref readonly DiagnosticContext context)
    {
        //     if (ParameterInfo.HasParens)
        //     {
        //         VerifySyntaxWithParens(in context);
        //         return;
        //     }
        //     if (ParameterInfo.Count.min > 0)
        //     {
        //         if (Parameters is null)
        //             context.Add("Expected parameters for block", _keyword.End);
        //     }
        //     else if (Parameters is not null)
        //         context.Add("Unexpected parameters for block", Parameters);

        //     if (OpeningBracket is null)
        //         context.Add("Expecting opening '{' bracket", _keyword.End);

        //     if (ClosingBracket is null)
        //         context.Add("Expected closing '}' bracket", End);
    }

    // TODO remove! params should be stored in right-associative expr AST
    // private List<ExpressionAST?> UnpackParams(ExpressionAST? root)
    // {
    //     List<ExpressionAST?> parameters = new(ParameterInfo.Count.min);
    //     while (root is BinaryOperation { Left: var left, OperatorType: OperatorType.Comma, Right: var right })
    //     {
    //         parameters.Add(right);
    //         root = left;
    //     }
    //     parameters.Add(root);
    //     return parameters;
    // }

    // private void VerifySyntaxWithParens(ref readonly DiagnosticContext context)
    // {
    //     if (Parameters is not ParentedExpression
    //         {
    //             OpeningBracket: var leftBracket,
    //             Expression: var expr,
    //             ClosingBracket: var rightBracket
    //         })
    //     {
    //         const string errorParamListExpected = "Expected parameter list enclosed in brackets";
    //         if (Parameters is null)
    //             context.Add(errorParamListExpected, _keyword.End);
    //         else
    //             context.Add(errorParamListExpected, Parameters);
    //         return;
    //     }
    //     if (leftBracket is null)
    //         context.Add("Missing opening '(' bracket", (expr?.Start - 1) ?? _keyword.End);
    //     if (rightBracket is null)
    //         context.Add("Missing closing ')' bracket", expr?.End ?? leftBracket?.End ?? _keyword.End);

    //     if (expr is null)
    //     {
    //         context.Add("Expected parameters", Parameters);
    //         return;
    //     }

    // var parameters = UnpackParams(expr);

    // if (ParameterInfo.Count.max > 0 && ParameterInfo.Count.max < parameters.Count)
    //     context.Add("Excess arguments", expr);

    // if (parameters.Count < ParameterInfo.Count.min)
    //     context.Add("Not enough parameters are present", expr);

    // TODO
    // var id = parameters[parameters.Count - 1 - ParameterInfo.symbolIndex];
    // switch (id)
    // {
    //     case null:
    //         const string errorExpectedId = "Expected block identifier.";
    //         // int leftOffset = 0;
    //         // TODO error
    //         break;
    //     case Identifier { kind: SymbolKind.Undefined } identifier:
    //         // symbol = identifier.Token;
    //         break;
    //     default:
    //         context.AddError(errorExpectedId, id);
    //         break;
    // }
    // }

    // public void VerifyContext(ref readonly DiagnosticContext context, DefinitionBag definitions)
    // {
    //     if (Symbol is null || !definitions.Has(Symbol, out var others))
    //         return;

    //     if (others.IndexOf(Symbol) != 0)
    //         context.Add("Identifier is already defined", Symbol);
    // }

    public override void ProvideFoldingRanges(in List<FoldingRange> ranges, ref readonly PositionConverter converter)
        => IFoldingRangeProvider.RangeFromBrackets(OpeningBracket, ClosingBracket, in ranges, in converter);

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = base.Visualize(graph, parent, ctx)
            .WithLabel("BlockStmt");

        _keyword.Visualize(graph, n, ctx);
        Parameters.MaybeVisualize(graph, n, ctx);
        OpeningBracket.MaybeVisualize(graph, n, ctx);
        return n;
    }
}

internal abstract class BaseBlockStatement<T> : BaseBlockStatement where T : IBlockContents<T>
{
    protected readonly List<T>? Contents;

    protected sealed override int? MiddleEnd => Contents?[^1].End;

    protected BaseBlockStatement(ref ParsingState state, KeywordToken keyword)
        : base(ref state, keyword)
    {
        if (ClosingBracket is null)
            Contents = T.ParseSomeInBlock(ref state, ref ClosingBracket);
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = base.Visualize(graph, parent, ctx);
        foreach (var item in Contents ?? [])
            item.Visualize(graph, n, ctx);
        ClosingBracket.MaybeVisualize(graph, n, ctx);
        return n;
    }
}