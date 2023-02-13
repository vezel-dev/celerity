namespace Vezel.Celerity.Generators.Semantics;

[XmlRoot("Tree", Namespace = "https://vezel.dev/celerity/SemanticTree")]
public sealed class SemanticTreeRoot
{
    [SuppressMessage("", "CA1819")]
    [XmlElement("Type")]
    public SemanticTreeType[] Types { get; set; } = null!;
}
