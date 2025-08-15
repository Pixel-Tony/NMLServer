namespace NMLServer.Model.Processors;

internal readonly ref struct NodeProcessingContext(StringView source, ref PositionConverter converter, bool isParent)
{
    public readonly StringView Source = source;
    public readonly ref PositionConverter Converter = ref converter;
    public readonly bool IsParent = isParent;
}