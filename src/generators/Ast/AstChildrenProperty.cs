namespace Vezel.Celerity.Generators.Ast;

public sealed class AstChildrenProperty : AstProperty
{
    [XmlAttribute]
    public string Type { get; set; } = null!;

    [XmlAttribute]
    public bool Separated { get; set; }

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
