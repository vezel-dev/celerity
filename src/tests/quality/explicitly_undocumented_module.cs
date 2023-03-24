public sealed partial class QualityTests
{
    [Fact]
    public Task explicitly_undocumented_module()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            @doc false
            mod {
                pub fn foo() {
                    "this is hidden because the module is";
                }
            }
            """,
            UndocumentedPublicDeclarationPass.Instance);
    }
}
