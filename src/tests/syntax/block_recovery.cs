public sealed partial class SyntaxTests
{
    [Fact]
    public Task block_recovery()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
                fn main() {
                    use  let;

                    42;

                    mod  const
                      type

                    assert
                    ;
                }
            }
            """);
    }
}
