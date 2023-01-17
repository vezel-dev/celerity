namespace Vezel.Celerity.Generators.Ast;

public sealed class AstChildProperty : AstProperty
{
    [XmlAttribute]
    public required string Type { get; init; }

    [XmlAttribute]
    public required bool Optional { get; init; }

    internal override string GetTypeName(AstTree tree)
    {
        var name = tree.GetNodeTypeName(Type);

        return Optional ? $"{name}?" : name;
    }
}
