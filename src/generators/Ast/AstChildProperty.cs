namespace Vezel.Celerity.Generators.Ast;

public sealed class AstChildProperty : AstProperty
{
    [XmlAttribute]
    public string Type { get; set; } = null!;

    [XmlAttribute]
    public bool Optional { get; set; }

    internal override string GetTypeName(AstTree tree)
    {
        var name = tree.GetNodeTypeName(Type);

        return Optional ? $"{name}?" : name;
    }
}
