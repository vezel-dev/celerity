namespace Vezel.Celerity.Runtime.Metadata;

public abstract class RuntimeMember : RuntimeMetadata
{
    public RuntimeModule Module { get; }

    public string Name { get; }

    private protected RuntimeMember(RuntimeModule module, string name, IEnumerable<AttributeSemantics> attributes)
        : base(attributes)
    {
        Module = module;
        Name = name;
    }
}
