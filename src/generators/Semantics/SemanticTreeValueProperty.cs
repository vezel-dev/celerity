// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Generators.Semantics;

public sealed class SemanticTreeValueProperty : SemanticTreeProperty
{
    [XmlAttribute]
    public required string Type { get; init; }

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
        var param = $"{char.ToLowerInvariant(Name[0])}{Name[1..]}";

        return SyntaxFacts.GetKeywordKind(param) != SyntaxKind.None ? $"@{param}" : param;
    }
}
