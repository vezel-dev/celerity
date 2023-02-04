namespace Vezel.Celerity.Generators.Ast;

[XmlRoot("Tree", Namespace = "https://vezel.dev/celerity/Ast")]
public sealed class AstTree
{
    [XmlElement]
    public required AstSettings Settings { get; init; }

    [SuppressMessage("", "CA1819")]
    [XmlElement("Type")]
    public AstType[] Types { get; set; } = null!;

    internal string GetTypeName(string name)
    {
        return $"{name}{Settings.NameSuffix}";
    }
}
