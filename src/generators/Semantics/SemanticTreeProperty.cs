// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Generators.Semantics;

public abstract class SemanticTreeProperty
{
    [XmlAttribute]
    public string Name { get; set; } = null!;

    [XmlAttribute]
    public bool Override { get; set; }

    internal abstract string GetTypeName();

    internal abstract string GetPropertyName();

    internal abstract string GetParameterName();
}
