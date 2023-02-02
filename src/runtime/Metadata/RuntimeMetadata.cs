namespace Vezel.Celerity.Runtime.Metadata;

public abstract class RuntimeMetadata
{
    public ImmutableArray<(string Name, object? Value)> Attributes { get; }

    private protected RuntimeMetadata(ImmutableArray<AttributePair> attributes)
    {
        Attributes = attributes.Select(pair => (pair.Name, pair.Value)).ToImmutableArray();
    }
}
