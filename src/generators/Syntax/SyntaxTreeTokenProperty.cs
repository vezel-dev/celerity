// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Generators.Syntax;

public sealed class SyntaxTreeTokenProperty : SyntaxTreeProperty
{
    [XmlAttribute]
    public required bool Optional { get; init; }

    public override bool CanContainNodes => false;

    public override bool CanContainTokens => true;

    internal override string GetTypeName()
    {
        return $"SyntaxToken{(Optional ? "?" : string.Empty)}";
    }

    internal override string GetPropertyName()
    {
        return $"{Name}Token";
    }

    internal override string GetParameterName()
    {
        return $"{char.ToLowerInvariant(Name[0])}{Name[1..]}Token";
    }
}
