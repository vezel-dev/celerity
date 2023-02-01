namespace Vezel.Celerity.Runtime.Metadata;

public sealed class RuntimeConstant : RuntimeMember
{
    internal RuntimeConstant(RuntimeModule module, ConstantDeclaration constant)
        : base(module, constant.Name, constant.Attributes)
    {
    }
}
