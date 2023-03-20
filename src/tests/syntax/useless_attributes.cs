public sealed partial class SyntaxTests
{
    [Fact]
    public Task useless_attributes()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
                fn foo() {
                    1;

                    @attr 2
                }

                fn bar() {
                    3;

                    @attr 4
                    @attr 5
                }

                @attr 2
            }
            """);
    }
}
