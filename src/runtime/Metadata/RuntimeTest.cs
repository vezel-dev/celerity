namespace Vezel.Celerity.Runtime.Metadata;

public sealed class RuntimeTest : RuntimeMember
{
    internal RuntimeTest(RuntimeModule module, TestDeclaration test)
        : base(module, test.Name, test.Attributes)
    {
    }
}
