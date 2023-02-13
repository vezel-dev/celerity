namespace Vezel.Celerity.Runtime.Metadata;

public sealed class RuntimeTest : RuntimeMember
{
    internal RuntimeTest(RuntimeModule module, TestDeclarationSemantics test)
        : base(module, test.Symbol!.Name, test.Attributes)
    {
    }
}
