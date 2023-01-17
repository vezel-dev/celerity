namespace Vezel.Celerity.Generators.Ast;

public sealed class AstTokensProperty : AstProperty
{
    [XmlAttribute]
    public required bool Separated { get; init; }

    internal override string GetPropertyName()
    {
        return $"{base.GetPropertyName()}Tokens";
    }

    internal override string GetTypeName(AstTree tree)
    {
        var list = "SyntaxItemList";
        var token = "SyntaxToken";

        return Separated ? $"Separated{list}<{token}, {token}>" : $"{list}<{token}>";
    }
}
