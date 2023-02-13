namespace Vezel.Celerity.Generators.Syntax;

[XmlRoot("Tree", Namespace = "https://vezel.dev/celerity/SyntaxTree")]
public sealed class SyntaxTreeRoot
{
    [SuppressMessage("", "CA1819")]
    [XmlElement("Type")]
    public SyntaxTreeType[] Types { get; set; } = null!;
}
