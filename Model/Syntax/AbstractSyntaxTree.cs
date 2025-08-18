using NMLServer.Extensions;
using NMLServer.Model.Statements;
using NMLServer.Model.Statements.Blocks;
using NMLServer.Model.Tokens;

namespace NMLServer.Model.Syntax;

internal struct AbstractSyntaxTree
{
    public readonly IReadOnlyList<BaseToken> UnexpectedTokens => _unexpectedTokens;
    public TokenStorage Tokens;
    public List<BaseStatement> Nodes { readonly get; private set; }

    private readonly List<BaseToken> _unexpectedTokens = [];

    public AbstractSyntaxTree(string text)
    {
        Tokens = new TokenStorage(text);
        Nodes = [];
        ParsingState state = new(Tokens.Items);
        List<BaseStatement> children = Nodes;

        Stack<BaseParentStatement?> stack = [];
        BaseParentStatement? parent = null;
        while (state.CurrentToken is { } token)
        {
            BaseStatement node;
            BaseParentStatement? nodeAsParent = null;
            switch (token)
            {
                case KeywordToken { IsDefiningStatement: true } keywordToken:
                    node = Parser.ParseStatement(ref state, keywordToken, out nodeAsParent);
                    break;

                case BracketToken { Bracket: '}' } closingBracket when parent is not null:
                    parent.ClosingBracket = closingBracket;
                    parent.Children = parent.Children!.ToMaybeList();
                    parent = stack.Pop();
                    children = parent?.Children ?? Nodes;
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
            children.Add(node);
            if (nodeAsParent is { Children: { } innerChildren })
            {
                stack.Push(parent);
                parent = nodeAsParent;
                children = innerChildren;
            }
        }
        if (parent is not null)
            parent.Children = parent.Children!.ToMaybeList();
        _unexpectedTokens = state.UnexpectedTokens;
    }

    public BaseStatement? Amend(Range? range, string replacement, out TreeTraverser result)
    {
        var (oldResult, (firstChangedToken, firstUnchangedToken)) = Tokens.Amend(range, replacement);

        ParsingState state;
        AmendingTreeTraverser traverser;
        int startOffset;
        {
            ParentStack navigation = [];
            BaseParentStatement? parent = null;
            List<BaseStatement> children = Nodes;
            int index;

            var items = Tokens.Items;
            var offset = items.Count > 0 ? items[firstChangedToken].Start : 0;
            while (true)
            {
                index = children.FindLastBefore<IHasStart, int>(offset);
                if (index == -1
                    || children[index] is not BaseParentStatement { Children: { } grandChildren } newParent
                    || grandChildren[0].Start > offset)
                {
                    index = int.Max(index, 0);
                    break;
                }
                navigation.Add((parent, index + 1));
                parent = newParent;
                children = grandChildren;
            }
            traverser = new AmendingTreeTraverser(navigation, Nodes, parent, index);
            result = new TreeTraverser(new AmendingTreeTraverser(ref traverser));

            startOffset = traverser.Current?.Start ?? 0;
            var startIndex = items.FindWhereOffset<IHasStart, int>(startOffset, firstChangedToken);
            state = new(items, int.Max(startIndex, 0));
            // TODO
            // var endOffset = firstUnchangedToken != items.Count ? items[firstUnchangedToken].Start : sourceLength;
            // oldTraverser = MakeOldTraverser(endOffset, Nodes);
        }

        while (state.CurrentToken is { } token)
        {
            KeywordToken? keyword = null;
            BracketToken? closingBracket = null;
            switch (token)
            {
                case KeywordToken { IsDefiningStatement: true } keywordToken:
                    keyword = keywordToken;
                    break;

                case BracketToken { Bracket: '}' } closingBracketToken when !traverser.TopLevel:
                    closingBracket = closingBracketToken;
                    break;

                case IdentifierToken:
                case BracketToken { Bracket: '(' }:
                case KeywordToken { IsExpressionUsable: true }:
                    break;

                default:
                    state.AddUnexpected(token);
                    state.Increment();
                    continue;
            }
            var offset = token.Start;
            traverser.DropUntil(offset);
            if (closingBracket is not null)
            {
                traverser.Ascend(closingBracket);
                state.Increment();
                continue;
            }
            var node = keyword is not null
                ? Parser.ParseStatement(ref state, keyword, out _)
                : new Assignment(ref state);

            traverser.Insert(node);
            traverser.Increment();
        }
        traverser.Trim();
        _unexpectedTokens.ReplaceRange(state.UnexpectedTokens,
            int.Max(0, _unexpectedTokens.FindLastBefore<IHasEnd, int>(startOffset)));
        return null;

        // static Traverser MakeOldTraverser(int offset, List<BaseStatement> nodes)
        // {
        //     ParentStack navigation = [];
        //     BaseParentStatement? parent = null;
        //     for (var children = nodes; ;)
        //     {
        //         var childIndex = children.FindFirstNotBefore(offset);
        //         if (childIndex == children.Count
        //             || children[childIndex] is not BaseParentStatement { Children: { } grandChildren } newParent)
        //         {
        //             navigation.Push((parent, childIndex));
        //             break;
        //         }
        //         navigation.Push((parent, childIndex + 1));
        //         parent = newParent;
        //         children = grandChildren;
        //     }
        //     return new Traverser(navigation, nodes);
        // }
    }
}