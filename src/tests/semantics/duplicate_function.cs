public sealed partial class SemanticTests
{
    [Fact]
    public Task duplicate_function()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
                fn foo() { 1; }
                fn foo() { 2; }
            }
            """);
    }
}
