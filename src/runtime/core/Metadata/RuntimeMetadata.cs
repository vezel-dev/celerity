namespace Vezel.Celerity.Runtime.Metadata;

public abstract class RuntimeMetadata
{
    public ImmutableArray<(string Name, object? Value)> Attributes { get; }

    private protected RuntimeMetadata(IEnumerable<AttributeSemantics> attributes)
    {
        Attributes = attributes.Select(static pair => (pair.Name, pair.Value)).ToImmutableArray();
    }
}
