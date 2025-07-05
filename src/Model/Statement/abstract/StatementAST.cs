using DotNetGraph.Core;
using NMLServer.Extensions;
using NMLServer.Model.Expression;

namespace NMLServer.Model.Statement;

internal abstract class StatementAST : IHasStart, IHasEnd, IVisualProvider
{
    public abstract int Start { get; }
    public abstract int End { get; }

    protected static class Errors
    {
        public const string ExpectedSquareBracket = "Expected square bracket for var/param invocation";
        public const string InvalidTarget = "Invalid parameter assignment target";
        public const string MissingSemicolon = "Missing semicolon";
        public const string MissingClosingBracket = "Missing closing bracket";
        public const string MissingAssignedValue = "Missing assigned value";
        public const string UnexpectedTopLevelExpr = "Unexpected expression at top level of the script";
    }

    public virtual DotNode Visualize(DotGraph graph, DotNode parent, string ctx)
        => VizExtensions.MakeNode(graph, parent, label: "Stmt").WithStmtFeatures();
}