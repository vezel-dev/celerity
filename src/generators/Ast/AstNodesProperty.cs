namespace Vezel.Celerity.Generators.Ast;

public sealed class AstNodesProperty : AstProperty
{
    [XmlAttribute]
    public required string Type { get; init; }

    [XmlAttribute]
    public required bool Separated { get; init; }

    internal override string GetPropertyName()
    {
        return $"{base.GetPropertyName()}s";
    }

    internal override string GetTypeName(AstTree tree)
    {
        var list = "SyntaxItemList";
        var type = tree.GetTypeName(Type);

        return Separated ? $"Separated{list}<{type}, SyntaxToken>" : $"{list}<{type}>";
    }
}
