namespace Vezel.Celerity.Generators.Semantics;

public sealed class SemanticTreeComputedProperty : SemanticTreeProperty
{
    [XmlAttribute]
    public required string Type { get; init; }

    [XmlAttribute]
    public required string Body { get; init; }

    internal override string GetTypeName()
    {
        return Type;
    }
}
