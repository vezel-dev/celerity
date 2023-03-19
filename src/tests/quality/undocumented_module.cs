public sealed partial class QualityTests
{
    [Fact]
    public Task undocumented_module()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
            }
            """,
            UndocumentedPublicDeclarationPass.Instance);
    }
}
