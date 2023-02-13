namespace Vezel.Celerity.Generators.Semantics;

public sealed class SemanticTreeNodeProperty : SemanticTreeProperty
{
    [XmlAttribute]
    public required string Type { get; init; }

    [XmlAttribute]
    public required bool Optional { get; init; }

    internal override string GetTypeName()
    {
        return $"{Type}Semantics{(Optional ? "?" : string.Empty)}";
    }
}
