// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Generators.Semantics;

public sealed class SemanticTreeType
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
    [XmlElement("Value", Type = typeof(SemanticTreeValueProperty))]
    [XmlElement("Node", Type = typeof(SemanticTreeNodeProperty))]
    [XmlElement("Nodes", Type = typeof(SemanticTreeNodesProperty))]
    [XmlElement("Computed", Type = typeof(SemanticTreeComputedProperty))]
    public required SemanticTreeProperty[] Properties { get; init; }
}
