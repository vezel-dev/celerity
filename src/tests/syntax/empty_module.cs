public sealed partial class SyntaxTests
{
    [Fact]
    public Task empty_module()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
            }
            """);
    }
}
