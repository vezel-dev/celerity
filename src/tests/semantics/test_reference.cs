public sealed partial class SemanticTests
{
    [Fact]
    public Task test_reference()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
                test foo { assert true; }

                fn bar() { foo; }
            }
            """);
    }
}
