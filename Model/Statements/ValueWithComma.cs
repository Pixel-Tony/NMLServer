using NMLServer.Extensions;
using NMLServer.Model.Tokens;
using DotNetGraph.Core;
using NMLServer.Model.Grammar;

namespace NMLServer.Model.Statements;

internal readonly struct ValueWithComma<T>(T identifier, BinaryOpToken? comma)
    : IBlockContents<ValueWithComma<T>> where T : BaseValueToken
{
    public int End => comma?.End ?? identifier.End;

    public static List<ValueWithComma<T>>? ParseSomeInBlock(ref ParsingState state, ref BracketToken? closingBracket)
    {
        List<ValueWithComma<T>> chain = [];
        T? current = null;
        while (state.CurrentToken is { } token)
        {
            switch (token)
            {
                case BinaryOpToken { Type: OperatorType.Comma } commaToken when current is not null:
                    chain.Add(new ValueWithComma<T>(current, commaToken));
                    state.Increment();
                    current = null;
                    break;

                case T value when current is null:
                    current = value;
                    state.Increment();
                    break;

                case BracketToken { Bracket: '}' } expectedClosingBracket:
                    closingBracket = expectedClosingBracket;
                    state.Increment();
                    goto label_End;

                case KeywordToken { IsDefiningStatement: true }:
                    goto label_End;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    break;
            }
        }
    label_End:
        return chain.ToMaybeList();
    }

    public DotNode Visualize(DotGraph graph, DotNode parent, StringView ctx)
    {
        var n = VizExtensions.MakeNode(graph, parent, "ValueComma").WithStmtFeatures();
        identifier.Visualize(graph, n, ctx);
        comma.MaybeVisualize(graph, n, ctx);
        return n;
    }
}