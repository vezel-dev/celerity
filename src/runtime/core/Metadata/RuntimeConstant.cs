namespace Vezel.Celerity.Runtime.Metadata;

public sealed class RuntimeConstant : RuntimeMember
{
    internal RuntimeConstant(RuntimeModule module, ConstantDeclarationSemantics constant)
        : base(module, constant.IsPublic, constant.Symbol!.Name, constant.Attributes)
    {
    }
}
