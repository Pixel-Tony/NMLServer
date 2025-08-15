using NMLServer.Extensions;
using NMLServer.Model.Statements;
using NMLServer.Model.Statements.Blocks;
using NMLServer.Model.Tokens;
using System.Runtime.InteropServices;

namespace NMLServer.Model.Syntax;

internal struct AmendingTreeTraverser(ParentStack navigation, List<BaseStatement> nodes, BaseParentStatement? parent, int index)
{
    private int _index = index;
    private BaseParentStatement? _parent = parent;
    private List<BaseStatement> _children = parent?.Children ?? nodes;
    private readonly List<BaseStatement> _nodes = nodes;
    public readonly ParentStack Parents = navigation;

    public readonly bool TopLevel => _children == _nodes;
    public readonly BaseStatement? Current => (_index >= 0) & (_index < _children.Count) ? _children[_index] : null;

    public readonly bool IsCurrentLastChild => _index == _children.Count - 1;

    public AmendingTreeTraverser(ref AmendingTreeTraverser other)
        : this([.. other.Parents], other._nodes, other._parent, other._index)
    { }

    public readonly void DropUntil(int offset)
    {
        while (Current is { Start: var oldOffset } oldCurrent && oldOffset <= offset)
        {
            if (oldCurrent is BaseParentStatement { Children: { } grandChildren })
            {
                _children.ReplaceRange((_index, _index + 1), grandChildren);
                continue;
            }
            _children.RemoveAt(_index);
        }
    }

    public readonly void Insert(BaseStatement node) => _children.Insert(_index, node);

    public void Increment()
    {
        if (Current is BaseParentStatement { Children: { } grandChildren } child)
        {
            Parents.Add((_parent, _index + 1));
            _parent = child;
            _children = grandChildren;
            _index = 0;
            return;
        }
        ++_index;
    }
    public void Ascend()
    {
        _parent!.Children = _children.ToMaybeList();
        (_parent, _index) = Parents.Pop();
        _children = _parent?.Children ?? _nodes;
    }

    public void Ascend(BracketToken closingBracket)
    {
        var (grandParent, indexAfterParent) = Parents.Pop();
        var parentNodes = grandParent?.Children ?? _nodes;
        // Move children in front of parent to the upper level
        if (_index != _children.Count)
        {
            var span = CollectionsMarshal.AsSpan(_children)[_index..];
            parentNodes.InsertRange(indexAfterParent, span);
            _children.RemoveRange(_index, _children.Count - _index);
        }
        _parent!.ClosingBracket = closingBracket;
        _parent!.Children = _children.ToMaybeList();
        _parent = grandParent;
        _children = parentNodes;
        _index = indexAfterParent;
    }

    public void Trim()
    {
        while (!TopLevel)
        {
            _children.RemoveRange(_index, _children.Count - _index);
            _parent!.ClosingBracket = null;
            _parent.Children = _children.ToMaybeList();
            (_parent, _index) = Parents.Pop();
            _children = _parent?.Children ?? _nodes;
        }
        _children.RemoveRange(_index, _children.Count - _index);
    }
}
