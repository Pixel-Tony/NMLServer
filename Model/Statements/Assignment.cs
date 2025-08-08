using NMLServer.Model.Tokens;
using NMLServer.Model.Expressions;
using NMLServer.Extensions;

using DotNetGraph.Core;
using DotNetGraph.Extensions;

namespace NMLServer.Model.Statements;

internal sealed class Assignment : BaseStatement
{
    private readonly BaseExpression _lhs;
    private readonly AssignmentToken? _equalsSign;
    private readonly BaseExpression? _value;
    private readonly SemicolonToken? _semicolon;

    public override int Start => _lhs.Start;
    public override int End => _semicolon?.End ?? _value?.End ?? _equalsSign?.End ?? _lhs.End;

    public Assignment(ref ParsingState state)
    {
        _lhs = BaseExpression.TryParse(ref state)!;
        while (state.CurrentToken is { } token)
        {
            switch (token)
            {
                case BracketToken { Bracket: '{' or '}' }:
                case KeywordToken { IsDefiningStatement: true }:
                    return;
                case AssignmentToken equalsSign:
                    _equalsSign = equalsSign;
                    state.Increment();
                    _value = BaseExpression.TryParse(ref state);
                    _semicolon = state.ExpectSemicolon();
                    return;
                case SemicolonToken semicolon:
                    _semicolon = semicolon;
                    state.Increment();
                    return;
                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
    }

    // public void VerifySyntax(ref readonly DiagnosticContext context)
    // {
    //     if (_equalsSign is null)
    //     {
    //         context.Add(Errors.UnexpectedTopLevelExpr, _lhs);
    //         return;
    //     }
    //     switch (_lhs)
    //         {
    //             // TODO behavior for previously defined parameters
    //             case Identifier:
    //                 break;

    //             case FunctionCall
    //             {
    //                 Function: KeywordToken { Keyword: Keyword.Param },
    //                 Arguments: { OpeningBracket: var opening, ClosingBracket: var closing } arguments
    //             }:
    //                 if (opening!.Bracket is not '[')
    //                     context.Add(Errors.ExpectedSquareBracket, opening);
    //                 if (closing is null)
    //                     context.Add(Errors.MissingClosingBracket, arguments.End);
    //                 else if (closing.Bracket is not ']')
    //                     context.Add(Errors.ExpectedSquareBracket, closing.Start);
    //                 break;

    //             default:
    //                 context.Add(Errors.InvalidTarget, _lhs);
    //                 break;
    //         }
    //     if (_value is null)
    //     {
    //         var pos = _equalsSign.End;
    //         context.Add(Errors.MissingAssignedValue, pos);
    //         if (_semicolon is null)
    //             context.Add(Errors.MissingSemicolon, pos);
    //         return;
    //     }

    //     _value.VerifySyntax(in context);
    //     if (_semicolon is null)
    //         context.Add(Errors.MissingSemicolon, _value.End);
    // }

    public override DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
    {
        var n = base.Visualize(graph, parent, ctx).WithLabel("Assignment");
        _lhs.Visualize(graph, n, ctx);
        _equalsSign.MaybeVisualize(graph, n, ctx);
        _value.MaybeVisualize(graph, n, ctx);
        _semicolon.MaybeVisualize(graph, n, ctx);
        return n;
    }
}