namespace Vezel.Celerity.Runtime.Metadata;

public abstract class RuntimeMetadata
{
    public ImmutableSortedDictionary<string, object?> Attributes { get; }

    private protected RuntimeMetadata(ImmutableArray<AttributePair> attributes)
    {
        Attributes = attributes.ToImmutableSortedDictionary(pair => pair.Name, pair => pair.Value);
    }
}
