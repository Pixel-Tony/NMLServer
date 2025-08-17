using NMLServer.Extensions;
using NMLServer.Model.Statements;
using NMLServer.Model.Statements.Blocks;

namespace NMLServer.Model.Syntax;

internal struct TreeTraverser(AmendingTreeTraverser data)
{
    private AmendingTreeTraverser _data = data;

    public readonly ReadOnlySpan<(BaseParentStatement? parent, int index)> Navigation => _data.Parents.ToReadOnlySpan();

    public readonly BaseStatement? Current => _data.Current;

    public readonly bool IsOnLastChild => _data.IsOnLastChild;

    public TreeTraverser(ref readonly AbstractSyntaxTree tree) : this(new AmendingTreeTraverser(in tree))
    { }

    public TreeTraverser(TreeTraverser other) : this(new AmendingTreeTraverser(ref other._data))
    { }

    public void Increment()
    {
        _data.Increment();
        if (_data.Current is null && !_data.TopLevel)
            _data.Ascend();
    }
}