using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Extensions;
using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal abstract class BlockStatement : StatementAST, IDiagnosticProvider, ISymbolSource, IContextProvider
{
    protected readonly record struct ParamInfo(int minParams, int maxParams, int symbolIndex, bool areParensPresent)
    {
        public static readonly ParamInfo None = new(0, 0, -1, false);
    }

    protected readonly ParamInfo ParameterInfo;
    private readonly KeywordToken _keyword;
    protected readonly ExpressionAST? Parameters;
    protected readonly BracketToken? OpeningBracket;
    public BracketToken? ClosingBracket;

    public sealed override int start => _keyword.start;

    public sealed override int end
    {
        get
        {
            if (ClosingBracket is not null)
            {
                return ClosingBracket.end;
            }
            var middle = middleEnd;
            return middle > 0 ? middle : OpeningBracket?.end ?? Parameters?.end ?? _keyword.end;
        }
    }

    protected abstract int middleEnd { get; }

    protected BlockStatement(ref ParsingState state, KeywordToken keyword, ParamInfo info)
    {
        ParameterInfo = info;
        _keyword = keyword;
        state.IncrementSkippingComments();
        Parameters = ExpressionAST.TryParse(ref state);
        for (var token = state.currentToken; token is not null; token = state.nextToken)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' } openingBracket:
                    OpeningBracket = openingBracket;
                    state.IncrementSkippingComments();
                    return;

                case BracketToken { Bracket: '}' } closingBracket:
                    ClosingBracket = closingBracket;
                    state.IncrementSkippingComments();
                    return;

                case KeywordToken { Kind: KeywordKind.BlockDefining or KeywordKind.CallDefining }:
                    return;

                default:
                    state.AddUnexpected(token);
                    break;
            }
        }
    }

    public IdentifierToken? symbol
    {
        get
        {
            if (ParameterInfo.symbolIndex < 0)
                return null;

            if (Parameters is not ParentedExpression { Expression: var expr })
                return null;

            var parameters = UnpackParams(expr);
            if (ParameterInfo.symbolIndex > parameters.Count - 1)
                return null;

            return parameters[^(ParameterInfo.symbolIndex + 1)] switch
            {
                Identifier { kind: SymbolKind.Undefined, token: var token } => token,
                _ => null
            };
        }
    }

    public virtual void VerifySyntax(ref readonly DiagnosticContext context)
    {
        if (ParameterInfo.areParensPresent)
        {
            VerifySyntaxWithParens(in context);
            return;
        }
        if (ParameterInfo.minParams > 0)
        {
            if (Parameters is null)
                context.Add("Expected parameters for block", _keyword.end);
        }
        else if (Parameters is not null)
            context.Add("Unexpected parameters for block", Parameters);

        if (OpeningBracket is null)
            context.Add("Expecting opening '{' bracket", _keyword.end);

        if (ClosingBracket is null)
            context.Add("Expected closing '}' bracket", end);
    }

    // TODO remove! params should be stored in right-associative expr AST
    private List<ExpressionAST?> UnpackParams(ExpressionAST? root)
    {
        List<ExpressionAST?> parameters = new(ParameterInfo.minParams);
        while (root is BinaryOperation { Left: var left, operatorType: OperatorType.Comma, Right: var right })
        {
            parameters.Add(right);
            root = left;
        }
        parameters.Add(root);
        return parameters;
    }

    private void VerifySyntaxWithParens(ref readonly DiagnosticContext context)
    {
        if (Parameters is not ParentedExpression
            {
                OpeningBracket: var leftBracket,
                Expression: var expr,
                ClosingBracket: var rightBracket
            })
        {
            const string errorParamListExpected = "Expected parameter list enclosed in brackets";
            if (Parameters is null)
                context.Add(errorParamListExpected, _keyword.end);
            else
                context.Add(errorParamListExpected, Parameters);
            return;
        }
        if (leftBracket is null)
            context.Add("Missing opening '(' bracket", (expr?.start - 1) ?? _keyword.end);
        if (rightBracket is null)
            context.Add("Missing closing ')' bracket", expr?.end ?? leftBracket?.end ?? _keyword.end);

        if (expr is null)
        {
            context.Add("Expected parameters", Parameters);
            return;
        }

        var parameters = UnpackParams(expr);

        if (ParameterInfo.maxParams > 0 && ParameterInfo.maxParams < parameters.Count)
            context.Add("Excess arguments", expr);

        if (parameters.Count < ParameterInfo.minParams)
            context.Add("Not enough parameters are present", expr);

        // TODO
        // var id = parameters[parameters.Count - 1 - ParameterInfo.symbolIndex];
        // switch (id)
        // {
        //     case null:
        //         const string errorExpectedId = "Expected block identifier.";
        //         // int leftOffset = 0;
        //         // TODO error
        //         break;
        //
        //     case Identifier { kind: SymbolKind.Undefined } identifier:
        //         // symbol = identifier.Token;
        //         break;
        //
        //     default:
        //         context.AddError(errorExpectedId, id);
        //         break;
        // }
    }

    public void VerifyContext(ref readonly DiagnosticContext context, IDefinitionsBag definitions)
    {
        if (symbol is not { } definition)
            return;

        if (!definitions.Has(definition, out var others))
            return;

        if (others.IndexOf(symbol) != 0)
            context.Add("Identifier is already defined", definition);
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = base.Visualize(graph, parent, ctx)
            .WithLabel("StmtWithBlock");

        _keyword.Visualize(graph, n, ctx);
        Parameters.MaybeVisualize(graph, n, ctx);
        OpeningBracket.MaybeVisualize(graph, n, ctx);
        return n;
    }
}

internal abstract class BlockStatement<T> : BlockStatement where T : IAllowsParseInsideBlock<T>
{
    protected readonly List<T>? Contents;

    protected sealed override int middleEnd => Contents?[^1].end ?? 0;

    protected BlockStatement(ref ParsingState state, KeywordToken keyword, ParamInfo info) : base(ref state,
        keyword, info)
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