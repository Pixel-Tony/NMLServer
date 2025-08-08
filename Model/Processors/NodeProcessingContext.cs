namespace NMLServer.Model.Processors;

internal ref struct NodeProcessingContext(StringView source, bool isParent, ref readonly PositionConverter converter)
{
    public readonly StringView Source = source;
    public ref readonly PositionConverter Converter = ref converter;
    public readonly bool IsParent = isParent;
}