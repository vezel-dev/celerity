public sealed partial class QualityTests
{
    [Fact]
    public Task undocumented_module()
    {
        return TestAsync(
            """
            mod {
            }
            """,
            UndocumentedPublicSymbolPass.Instance);
    }
}
