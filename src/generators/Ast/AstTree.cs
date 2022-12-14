namespace Vezel.Celerity.Generators.Ast;

[XmlRoot("Tree", Namespace = "https://vezel.dev/celerity/Ast")]
public sealed class AstTree
{
    [XmlElement]
    public AstSettings Settings { get; set; } = null!;

    [SuppressMessage("", "CA1819")]
    [XmlElement("Node")]
    public AstNode[] Nodes { get; set; } = null!;

    internal string GetNodeTypeName(string name)
    {
        return $"{name}{Settings.NameSuffix}";
    }
}
