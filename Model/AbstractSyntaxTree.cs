using NMLServer.Extensions;
using NMLServer.Model.Statements;
using NMLServer.Model.Statements.Blocks;
using NMLServer.Model.Tokens;

namespace NMLServer.Model;

internal struct AbstractSyntaxTree
{
    public readonly IReadOnlyList<BaseToken> UnexpectedTokens => _unexpectedTokens;
    public TokenStorage Tokens;
    public List<BaseStatement> Nodes { readonly get; private set; }

    private List<BaseToken> _unexpectedTokens = [];

    public AbstractSyntaxTree(string text)
    {
        Tokens = new TokenStorage(text);
        Nodes = [];
        ParsingState state = new(Tokens.Items);
        List<BaseStatement> _children = Nodes;
        BaseParentStatement? _current = null;
        while (state.CurrentToken is { } token)
        {
            BaseStatement node;
            BaseParentStatement? parent = null;
            switch (token)
            {
                case KeywordToken { IsDefiningStatement: true } keywordToken:
                    node = Parser.ParseStatement(_current, ref state, keywordToken, out parent);
                    break;

                case BracketToken { Bracket: '}' } closingBracket when _current is not null:
                    _current.ClosingBracket = closingBracket;
                    _current.Children = _current.Children!.ToMaybeList();
                    _current = _current.Parent;
                    _children = _current?.Children ?? Nodes;
                    state.Increment();
                    continue;

                case IdentifierToken:
                case BracketToken { Bracket: '(' }:
                case KeywordToken { IsExpressionUsable: true }:
                    node = new Assignment(ref state);
                    break;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    continue;
            }
            _children.Add(node);
            if (parent is { Children: { } innerChildren })
            {
                _current = parent;
                _children = innerChildren;
            }
        }
        if (_current is not null)
            _current.Children = _current.Children!.ToMaybeList();
        _unexpectedTokens = state.UnexpectedTokens;
    }

    public (BaseStatement?, BaseStatement?) Amend(Range? range, string replacement)
    {
        var (firstChangedToken, firstUnchangedToken) = Tokens.Amend(range, replacement);
        var (changedParent, changedChildIndex) = FindStartForAmend(firstChangedToken);

        var startOffset = changedChildIndex == -1
            ? (changedParent is null ? 0 : changedParent.Start)
            : (changedParent?.Children ?? Nodes)[changedChildIndex].Start;
        var startIndex = Tokens.Items.FindWhereStart(startOffset, firstChangedToken);

        ParsingState state = new(Tokens.Items, startIndex);

        var (unchangedParent, unchangedChildIndex) = FindEndForAmend(firstUnchangedToken);


        // TODO
        // while (state.CurrentToken is { } token)
        // {
        //     switch (token)
        //     {
        //         default:
        //             state.AddUnexpected(token);
        //             state.Increment();
        //             break;
        //     }
        // }

        // TODO
        var x = new AbstractSyntaxTree(Tokens.Source);
        Nodes = x.Nodes;
        _unexpectedTokens = x._unexpectedTokens;
        return (null, null);
    }

    private readonly (BaseParentStatement? parent, int childIndex) FindStartForAmend(int firstChangedToken)
    {
        int offset = Tokens.Items.Count > 0 ? Tokens.Items[firstChangedToken].Start : 0;
        BaseParentStatement? parent = null;
        int childIndex;
        for (var children = Nodes; ;)
        {
            childIndex = children.FindLastNotAfter(offset);
            if (childIndex == -1)
                break;
            var node = children[childIndex];
            if (node is not BaseParentStatement { Children: { } grandChildren } newParent)
                break;
            parent = newParent;
            children = grandChildren;
        }
        return (parent, childIndex);
    }

    private readonly (BaseParentStatement? parent, int childIndex) FindEndForAmend(int firstUnchangedToken)
    {
        if (firstUnchangedToken == Tokens.Items.Count)
            return (null, Nodes.Count);
        int offset = Tokens.Items[firstUnchangedToken].Start;

        BaseParentStatement? parent = null;
        int childIndex;
        for (var children = Nodes; ;)
        {
            childIndex = children.FindFirstAfter(offset);
            if (childIndex == -1)
                break;
            var node = children[childIndex];
            if (node is not BaseParentStatement { Children: { } grandChildren } newParent)
                break;
            parent = newParent;
            children = grandChildren;
        }
        return (parent, childIndex);
    }
}