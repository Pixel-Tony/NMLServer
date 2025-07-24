using DotNetGraph.Core;
using NMLServer.Extensions;
using NMLServer.Model.Diagnostics;
using NMLServer.Model.Lexis;

namespace NMLServer.Model.Expression;

internal sealed class ParentedExpression(
    ExpressionAST? parent = null,
    BracketToken? openingBracket = null,
    ExpressionAST? expression = null,
    BracketToken? closingBracket = null)
    : ExpressionAST(parent)
{
    public readonly BracketToken? OpeningBracket = openingBracket;
    public ExpressionAST? Expression = expression;
    public BracketToken? ClosingBracket = closingBracket;

    public override int Start => OpeningBracket?.Start ?? Expression?.Start ?? ClosingBracket!.Start;

    public override int End => ClosingBracket?.End ?? Expression?.End ?? OpeningBracket!.End;

    protected override void Replace(ExpressionAST target, FunctionCall value)
    {
        Expression = value;
    }

    public bool Matches(BracketToken closingBracket)
    {
        return closingBracket.Bracket == OpeningBracket!.Bracket switch
        {
            '(' => ')',
            '[' => ']',
            '{' => '}',
            _ => '\0'
        };
    }

    public override void VerifySyntax(ref readonly DiagnosticContext context)
    {
        if (OpeningBracket is null)
            context.Add("Missing opening paren", Start);
        else if (ClosingBracket is null)
            context.Add("Missing closing paren", End);

        if (Expression is null)
        {
            context.Add("Missing expression in parens", this);
            return;
        }
        Expression.VerifySyntax(in context);
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "ParentedExpr")
            .WithExprStyle();
        OpeningBracket.MaybeVisualize(graph, n, ctx);
        Expression.MaybeVisualize(graph, n, ctx);
        ClosingBracket.MaybeVisualize(graph, n, ctx);
        return n;
    }
}