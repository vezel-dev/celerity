namespace Vezel.Celerity.Generators.Syntax;

public sealed class SyntaxTreeNodeProperty : SyntaxTreeProperty
{
    [XmlAttribute]
    public required string Type { get; init; }

    [XmlAttribute]
    public required bool Optional { get; init; }

    public override bool CanContainNodes => true;

    public override bool CanContainTokens => false;

    internal override string GetTypeName()
    {
        return $"{Type}Syntax{(Optional ? "?" : string.Empty)}";
    }
}
