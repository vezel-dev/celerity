namespace Vezel.Celerity.Generators.Syntax;

public sealed class SyntaxTreeNodesProperty : SyntaxTreeProperty
{
    [XmlAttribute]
    public required string Type { get; init; }

    [XmlAttribute]
    public required bool Separated { get; init; }

    public override bool CanContainNodes => true;

    public override bool CanContainTokens => Separated;

    internal override string GetTypeName()
    {
        var type = $"SyntaxItemList<{Type}Syntax>";

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
