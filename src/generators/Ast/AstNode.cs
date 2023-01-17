namespace Vezel.Celerity.Generators.Ast;

public sealed class AstNode
{
    [XmlAttribute]
    public required string Name { get; init; }

    [XmlAttribute]
    public required string? Base { get; init; }

    [XmlAttribute]
    public required bool Abstract { get; init; }

    [SuppressMessage("", "CA1819")]
    [XmlElement("Token", Type = typeof(AstTokenProperty))]
    [XmlElement("Tokens", Type = typeof(AstTokensProperty))]
    [XmlElement("Child", Type = typeof(AstChildProperty))]
    [XmlElement("Children", Type = typeof(AstChildrenProperty))]
    public required AstProperty[] Properties { get; init; }
}
