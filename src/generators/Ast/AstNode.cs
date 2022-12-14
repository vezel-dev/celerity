namespace Vezel.Celerity.Generators.Ast;

public sealed class AstNode
{
    [XmlAttribute]
    public string Name { get; set; } = null!;

    [XmlAttribute]
    public string? Base { get; set; }

    [XmlAttribute]
    public bool Abstract { get; set; }

    [SuppressMessage("", "CA1819")]
    [XmlElement("Token", Type = typeof(AstTokenProperty))]
    [XmlElement("Tokens", Type = typeof(AstTokensProperty))]
    [XmlElement("Child", Type = typeof(AstChildProperty))]
    [XmlElement("Children", Type = typeof(AstChildrenProperty))]
    public AstProperty[] Properties { get; set; } = null!;
}
