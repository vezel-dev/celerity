// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Runtime.Metadata;

public abstract class RuntimeMember : RuntimeMetadata
{
    public RuntimeModule Module { get; }

    public bool IsPublic { get; }

    public string Name { get; }

    private protected RuntimeMember(
        RuntimeModule module, bool isPublic, string name, IEnumerable<AttributeSemantics> attributes)
        : base(attributes)
    {
        Module = module;
        IsPublic = isPublic;
        Name = name;
    }
}
