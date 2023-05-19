namespace Vezel.Celerity.Runtime.Metadata;

public sealed class RuntimeTest : RuntimeMember
{
    internal RuntimeTest(RuntimeModule module, TestDeclarationSemantics test)
        : base(module, isPublic: false, test.Symbol!.Name, test.Attributes)
    {
    }

    public override string ToString()
    {
        return $"test {Module.Path}.{Name}";
    }
}
