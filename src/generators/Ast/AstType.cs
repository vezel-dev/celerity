namespace Vezel.Celerity.Generators.Ast;

public sealed class AstType
{
    [XmlAttribute]
    public required string Name { get; init; }

    [XmlAttribute]
    public required string? Base { get; init; }

    [XmlAttribute]
    public required bool Abstract { get; init; }

    [XmlAttribute]
    public required bool Root { get; init; }

    [SuppressMessage("", "CA1819")]
    [XmlElement("Token", Type = typeof(AstTokenProperty))]
    [XmlElement("Tokens", Type = typeof(AstTokensProperty))]
    [XmlElement("Node", Type = typeof(AstNodeProperty))]
    [XmlElement("Nodes", Type = typeof(AstNodesProperty))]
    public required AstProperty[] Properties { get; init; }
}
