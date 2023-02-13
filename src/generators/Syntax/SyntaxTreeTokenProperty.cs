namespace Vezel.Celerity.Generators.Syntax;

public sealed class SyntaxTreeTokenProperty : SyntaxTreeProperty
{
    [XmlAttribute]
    public required bool Optional { get; init; }

    public override bool CanContainNodes => false;

    public override bool CanContainTokens => true;

    internal override string GetTypeName()
    {
        return $"SyntaxToken{(Optional ? "?" : string.Empty)}";
    }

    internal override string GetPropertyName()
    {
        return $"{base.GetPropertyName()}Token";
    }
}
