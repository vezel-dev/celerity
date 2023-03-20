public sealed partial class SyntaxTests
{
    [Fact]
    public Task declaration_recovery()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
                42;

                mod;

                fn main() {
                    1;
                }

                assert false;
            }
            """);
    }
}
