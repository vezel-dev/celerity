public sealed partial class QualityTests
{
    [Fact]
    public Task explicitly_undocumented_module()
    {
        return TestAsync(
            """
            @doc false
            mod {
                pub fn foo() {
                    "this is hidden because the module is";
                }
            }
            """,
            UndocumentedPublicSymbolPass.Instance);
    }
}
