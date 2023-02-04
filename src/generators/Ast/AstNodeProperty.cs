namespace Vezel.Celerity.Generators.Ast;

public sealed class AstNodeProperty : AstProperty
{
    [XmlAttribute]
    public required string Type { get; init; }

    [XmlAttribute]
    public required bool Optional { get; init; }

    internal override string GetTypeName(AstTree tree)
    {
        var name = tree.GetTypeName(Type);

        return Optional ? $"{name}?" : name;
    }
}
