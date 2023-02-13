namespace Vezel.Celerity.Generators.Semantics;

public sealed class SemanticTreeValueProperty : SemanticTreeProperty
{
    [XmlAttribute]
    public required string Type { get; init; }

    internal override string GetTypeName()
    {
        return Type;
    }
}
