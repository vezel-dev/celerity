namespace Vezel.Celerity.Generators.Ast;

public sealed class AstSettings
{
    [XmlAttribute]
    public string Namespace { get; set; } = null!;

    [XmlAttribute]
    public string NameSuffix { get; set; } = null!;

    [XmlAttribute]
    public string BaseType { get; set; } = null!;
}
