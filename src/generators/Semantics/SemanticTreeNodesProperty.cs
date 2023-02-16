namespace Vezel.Celerity.Generators.Semantics;

public sealed class SemanticTreeNodesProperty : SemanticTreeProperty
{
    [XmlAttribute]
    public required string Type { get; init; }

    internal override string GetTypeName()
    {
        return $"SemanticNodeList<{Type}Semantics>";
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
