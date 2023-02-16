namespace Vezel.Celerity.Generators.Semantics;

public sealed class SemanticTreeComputedProperty : SemanticTreeProperty
{
    [XmlAttribute]
    public required string Type { get; init; }

    [XmlAttribute]
    public required string Body { get; init; }

    internal override string GetTypeName()
    {
        return Type;
    }

    internal override string GetPropertyName()
    {
        return Name;
    }

    internal override string GetParameterName()
    {
        // TODO: https://github.com/Sergio0694/PolySharp/issues/60
        throw new NotSupportedException();
    }
}
