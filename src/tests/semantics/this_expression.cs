public sealed partial class SemanticTests
{
    [Fact]
    public Task this_expression()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
                fn foo() {
                    fn() -> this;

                    this;
                }
            }
            """);
    }
}
