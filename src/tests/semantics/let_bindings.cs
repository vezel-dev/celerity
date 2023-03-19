public sealed partial class SemanticTests
{
    [Fact]
    public Task let_bindings()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
                fn bindings(arg) {
                    let a = b;
                    let c = a;

                    c = 1;

                    let mut c = c;

                    c = 2;

                    let d = arg;
                }
            }
            """);
    }
}
