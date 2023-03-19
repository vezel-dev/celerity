public sealed partial class SemanticTests
{
    [Fact]
    public Task error_handling()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
                fn infallible() {
                    raise err MyError { message : "uh oh" };

                    try raise err MyError { message : "this is fine" } catch { _ -> nil };

                    try nil catch { _ -> raise err MyError { message : "this is bad" } };

                    fallible()?;
                }

                err fn fallible() {
                    raise err MyError { message : "this is also fine" };

                    fallible()?;

                    fn() -> raise err MyError { message : "this lambda is infallible" };
                }
            }
            """);
    }
}
