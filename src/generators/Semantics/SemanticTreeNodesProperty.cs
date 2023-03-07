namespace Vezel.Celerity.Generators.Semantics;

public sealed class SemanticTreeNodesProperty : SemanticTreeProperty
{
    [XmlAttribute]
    public required string Type { get; init; }

    [XmlAttribute]
    public required bool Separated { get; init; }

    internal override string GetTypeName()
    {
        var type = $"SemanticNodeList<{Type}Semantics, {Type}Syntax>";

        return Separated ? $"Separated{type}" : type;
    }

    internal override string GetPropertyName()
    {
        return $"{Name}s";
    }

    internal override string GetParameterName()
    {
        var param = $"{char.ToLowerInvariant(Name[0])}{Name[1..]}s";

        return SyntaxFacts.GetKeywordKind(param) != SyntaxKind.None ? $"@{param}" : param;
    }
}
