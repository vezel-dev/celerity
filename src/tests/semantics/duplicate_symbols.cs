public sealed partial class SemanticTests
{
    [Fact]
    public Task duplicate_symbols()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
                fn foo() { 1; }

                const foo = 2;

                test foo { assert true; }
            }
            """);
    }
}
