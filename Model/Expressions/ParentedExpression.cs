using NMLServer.Model.Processors.Diagnostics;
using NMLServer.Model.Tokens;
using DotNetGraph.Core;
using NMLServer.Extensions;
using NMLServer.Model.Grammar;

namespace NMLServer.Model.Expressions;

internal sealed class ParentedExpression(
    BaseExpression? parent = null,
    BracketToken? openingBracket = null,
    BaseExpression? expression = null,
    BracketToken? closingBracket = null)
    : BaseExpression(parent)
{
    public readonly BracketToken? OpeningBracket = openingBracket;
    public BaseExpression? Expression = expression;
    public BracketToken? ClosingBracket = closingBracket;

    public override int Start => OpeningBracket?.Start ?? Expression?.Start ?? ClosingBracket!.Start;

    public override int End => ClosingBracket?.End ?? Expression?.End ?? OpeningBracket!.End;

    protected override void Replace(BaseExpression target, FunctionCall value) => Expression = value;

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

    public void ConvertToRightAssociative()
    {
        while (Expression is BinaryOperation
            {
                Left: BinaryOperation
                {
                    OperatorType: OperatorType.Comma,
                    Right: var m
                } x,
                OperatorType: OperatorType.Comma
            } y)
        {
            y.Left = m;
            if (m is not null)
                m.Parent = y;
            x.Right = y;
            y.Parent = x;
            x.Parent = this;
            Expression = x;
        }
    }

    public override void VerifySyntax(DiagnosticContext context)
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
        Expression.VerifySyntax(context);
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, StringView ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "ParentedExpr")
            .WithExprStyle();
        OpeningBracket.MaybeVisualize(graph, n, ctx);
        Expression.MaybeVisualize(graph, n, ctx);
        ClosingBracket.MaybeVisualize(graph, n, ctx);
        return n;
    }
}