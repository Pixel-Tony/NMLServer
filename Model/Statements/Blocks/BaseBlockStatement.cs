using NMLServer.Model.Tokens;
using NMLServer.Model.Expressions;
using EmmyLua.LanguageServer.Framework.Protocol.Message.FoldingRange;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using NMLServer.Extensions;
using NMLServer.Model.Processors.Diagnostics;
using NMLServer.Model.Grammar;
using System.Runtime.CompilerServices;
using NMLServer.Logging;

namespace NMLServer.Model.Statements.Blocks;

internal abstract class BaseBlockStatement : BaseStatement
{
    protected readonly KeywordToken Keyword;
    protected readonly BaseExpression? Arguments;
    public readonly BracketToken? OpeningBracket;
    public BracketToken? ClosingBracket;

    public sealed override int Start => Keyword.Start;

    public sealed override int End => ClosingBracket?.End ?? MiddleEnd ?? OpeningBracket?.End ?? Arguments?.End ?? Keyword.End;

    protected abstract int? MiddleEnd { get; }

    protected BaseBlockStatement(ref ParsingState state, KeywordToken keyword)
    {
        Keyword = keyword;
        state.Increment();
        Arguments = BaseExpression.TryParse(ref state);
        (Arguments switch
        {
            ParentedExpression parented => parented,
            FunctionCall call => call.Arguments,
            _ => null,
        })?.ConvertToRightAssociative();

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
    }

    public override void AddDefinitions(DefinitionBag bag, StringView source)
    {
        if (CaptureSymbol() is { Kind: SymbolKind.None } token)
            bag.Add(token, Keyword.SymbolKind, source);
    }

    protected virtual IdentifierToken? CaptureSymbol() => CaptureSymbol(Keyword, Arguments);

    public override void ProvideDiagnostics(DiagnosticContext context)
    {
        ProcessArguments(context);
        if (OpeningBracket is null)
            context.Add(ErrorStrings.Err_ExpectedLeftCurlyBracket, Keyword.End);
        if (ClosingBracket is null)
            context.Add(ErrorStrings.Err_ExpectedRightCurlyBracket, End);
    }

    protected virtual void ProcessArguments(DiagnosticContext context) => ProcessArgumentList(context, Keyword, Keyword.End, Arguments);

    public override void ProvideFoldingRanges(List<FoldingRange> ranges, ref PositionConverter converter)
        => IFoldingRangeProvider.RangeFromBrackets(OpeningBracket, ClosingBracket, ranges, ref converter);

    public override DotNode Visualize(DotGraph graph, DotNode parent, StringView ctx)
    {
        var n = base.Visualize(graph, parent, ctx)
            .WithLabel("BlockStmt");

        Keyword.Visualize(graph, n, ctx);
        Arguments.MaybeVisualize(graph, n, ctx);
        OpeningBracket.MaybeVisualize(graph, n, ctx);
        return n;
    }
}

internal abstract class BaseBlockStatement<T> : BaseBlockStatement where T : IBlockContents<T>
{
    protected readonly List<T>? Contents;

    protected sealed override int? MiddleEnd => Contents?[^1].End;

    protected BaseBlockStatement(ref ParsingState state, KeywordToken keyword) : base(ref state, keyword)
    {
        if (ClosingBracket is null)
            Contents = T.ParseSomeInBlock(ref state, ref ClosingBracket);
    }

    public override DotNode Visualize(DotGraph graph, DotNode parent, StringView ctx)
    {
        var n = base.Visualize(graph, parent, ctx);
        foreach (var item in Contents ?? [])
            item.Visualize(graph, n, ctx);
        ClosingBracket.MaybeVisualize(graph, n, ctx);
        return n;
    }
}