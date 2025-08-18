using NMLServer.Model.Tokens;
using NMLServer.Model.Expressions;
using NMLServer.Extensions;

using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Model.Processors.Diagnostics;
using NMLServer.Model.Grammar;

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

    public override void AddDefinitions(DefinitionBag map, StringView source)
    {
        if (_lhs is Identifier id)
            map.Add(id.Token, SymbolKind.Variable, source);
    }


    public override void ProvideDiagnostics(DiagnosticContext context)
    {
        if (_equalsSign is null)
        {
            context.Add(ErrorStrings.Err_UnexpectedTopLevelExpr, _lhs);
            return;
        }
        switch (_lhs)
        {
            case Identifier:
                break;

            case FunctionCall
            {
                Function: KeywordToken { Keyword: Keyword.Param },
                Arguments: { OpeningBracket: { } opening, ClosingBracket: var closing } arguments
            }:
                if (opening.Bracket is not '[')
                    context.Add(ErrorStrings.Err_ExpectedSquareBracket, opening);
                if (closing is null)
                    context.Add(ErrorStrings.Err_MissingClosingBracket, arguments.End);
                else if (closing.Bracket is not ']')
                    context.Add(ErrorStrings.Err_ExpectedSquareBracket, closing.Start);
                break;

            default:
                context.Add(ErrorStrings.Err_InvalidTarget, _lhs);
                break;
        }
        var pos = _equalsSign.End;
        if (_value is not null)
        {
            _value.VerifySyntax(context);
            pos = _value.End;
        }
        else
            context.Add(ErrorStrings.Err_MissingAssignedValue, pos);
        if (_semicolon is null)
            context.Add(ErrorStrings.Err_MissingSemicolon, pos);
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, StringView ctx)
    {
        var n = base.Visualize(graph, parent, ctx).WithLabel("Assignment");
        _lhs.Visualize(graph, n, ctx);
        _equalsSign.MaybeVisualize(graph, n, ctx);
        _value.MaybeVisualize(graph, n, ctx);
        _semicolon.MaybeVisualize(graph, n, ctx);
        return n;
    }
}