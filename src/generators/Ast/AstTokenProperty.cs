namespace Vezel.Celerity.Generators.Ast;

public sealed class AstTokenProperty : AstProperty
{
    [XmlAttribute]
    public required bool Optional { get; init; }

    internal override string GetPropertyName()
    {
        return $"{base.GetPropertyName()}Token";
    }

    internal override string GetTypeName(AstTree tree)
    {
        var name = "SyntaxToken";

        return Optional ? $"{name}?" : name;
    }
}
