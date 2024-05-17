// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Generators.Syntax;

public abstract class SyntaxTreeProperty
{
    [XmlAttribute]
    public string Name { get; set; } = null!;

    [XmlAttribute]
    public bool Override { get; set; }

    public abstract bool CanContainNodes { get; }

    public abstract bool CanContainTokens { get; }

    internal abstract string GetTypeName();

    internal abstract string GetPropertyName();

    internal abstract string GetParameterName();
}
