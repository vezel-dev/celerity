namespace Vezel.Celerity.Generators.Ast;

public abstract class AstProperty
{
    [XmlAttribute]
    public string Name { get; set; } = null!;

    [XmlAttribute]
    public bool Override { get; set; }

    internal virtual string GetPropertyName()
    {
        return Name;
    }

    internal string GetParameterName()
    {
        var param = $"{char.ToLowerInvariant(Name[0])}{Name.Substring(1)}";

        return SyntaxFacts.GetKeywordKind(param) != SyntaxKind.None ? $"@{param}" : param;
    }

    internal abstract string GetTypeName(AstTree tree);
}
