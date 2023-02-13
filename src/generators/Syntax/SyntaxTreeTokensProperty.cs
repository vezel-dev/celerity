namespace Vezel.Celerity.Generators.Syntax;

public sealed class SyntaxTreeTokensProperty : SyntaxTreeProperty
{
    [XmlAttribute]
    public required bool Separated { get; init; }

    public override bool CanContainNodes => false;

    public override bool CanContainTokens => true;

    internal override string GetTypeName()
    {
        return Separated ? "SeparatedSyntaxItemList<SyntaxToken, SyntaxToken>" : "SyntaxItemList<SyntaxToken>";
    }

    internal override string GetPropertyName()
    {
        return $"{base.GetPropertyName()}Tokens";
    }
}
