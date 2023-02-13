namespace Vezel.Celerity.Generators.Semantics;

public abstract class SemanticTreeProperty
{
    [XmlAttribute]
    public string Name { get; set; } = null!;

    [XmlAttribute]
    public bool Override { get; set; }

    internal abstract string GetTypeName();

    internal virtual string GetPropertyName()
    {
        return Name;
    }

    internal string GetParameterName()
    {
        var param = $"{char.ToLowerInvariant(Name[0])}{Name[1..]}";

        return SyntaxFacts.GetKeywordKind(param) != SyntaxKind.None ? $"@{param}" : param;
    }
}
