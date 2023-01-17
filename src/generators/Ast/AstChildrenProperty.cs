namespace Vezel.Celerity.Generators.Ast;

public sealed class AstChildrenProperty : AstProperty
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
        var type = tree.GetNodeTypeName(Type);

        return Separated ? $"Separated{list}<{type}, SyntaxToken>" : $"{list}<{type}>";
    }
}
