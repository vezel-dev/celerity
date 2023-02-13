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
        var type = $"{Type}Syntax";

        return Separated ? $"SeparatedSyntaxItemList<{type}, SyntaxToken>" : $"SyntaxItemList<{type}>";
    }

    internal override string GetPropertyName()
    {
        return $"{base.GetPropertyName()}s";
    }
}
