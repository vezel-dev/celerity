namespace Vezel.Celerity.Generators.Semantics;

public sealed class SemanticTreeNodesProperty : SemanticTreeProperty
{
    [XmlAttribute]
    public required string Type { get; init; }

    internal override string GetTypeName()
    {
        return $"SemanticNodeList<{Type}Semantics>";
    }

    internal override string GetPropertyName()
    {
        return $"{base.GetPropertyName()}s";
    }
}
