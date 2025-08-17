using NMLServer.Model.Syntax;

namespace NMLServer.Model.Processors;

internal readonly ref struct IncrementContext(StringView source, ref readonly AbstractSyntaxTree tree)
{
    public readonly StringView Source = source;
    public readonly ref readonly AbstractSyntaxTree SyntaxTree = ref tree;
}