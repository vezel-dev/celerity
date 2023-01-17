namespace Vezel.Celerity.Generators.Ast;

public sealed class AstSettings
{
    [XmlAttribute]
    public required string Namespace { get; init; }

    [XmlAttribute]
    public required string NameSuffix { get; init; }

    [XmlAttribute]
    public required string BaseType { get; init; }
}
