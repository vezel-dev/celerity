// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Generators.Syntax;

public sealed class SyntaxTreeType
{
    [XmlAttribute]
    public required string Name { get; init; }

    [XmlAttribute]
    public required string? Base { get; init; }

    [XmlAttribute]
    public required string? Parent { get; init; }

    [XmlAttribute]
    public required bool Abstract { get; init; }

    [XmlAttribute]
    public required bool Root { get; init; }

    [SuppressMessage("", "CA1819")]
    [XmlElement("Token", Type = typeof(SyntaxTreeTokenProperty))]
    [XmlElement("Tokens", Type = typeof(SyntaxTreeTokensProperty))]
    [XmlElement("Node", Type = typeof(SyntaxTreeNodeProperty))]
    [XmlElement("Nodes", Type = typeof(SyntaxTreeNodesProperty))]
    public required SyntaxTreeProperty[] Properties { get; init; }
}
