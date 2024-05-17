// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Generators.Syntax;

public sealed class SyntaxTreeTokensProperty : SyntaxTreeProperty
{
    [XmlAttribute]
    public required bool Separated { get; init; }

    public override bool CanContainNodes => false;

    public override bool CanContainTokens => true;

    internal override string GetTypeName()
    {
        return Separated ? "SeparatedSyntaxItemList<SyntaxToken>" : "SyntaxItemList<SyntaxToken>";
    }

    internal override string GetPropertyName()
    {
        return $"{Name}Tokens";
    }

    internal override string GetParameterName()
    {
        return $"{char.ToLowerInvariant(Name[0])}{Name[1..]}Tokens";
    }
}
