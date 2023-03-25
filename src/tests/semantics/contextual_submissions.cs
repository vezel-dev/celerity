public sealed partial class SemanticTests
{
    [Fact]
    public Task contextual_submissions()
    {
        return TestAsync(
            ctx => ctx
                .AddSymbol("mutable", mutable: true)
                .AddSymbol("immutable", mutable: false),
            """
            mutable = 1;
            immutable = 2;
            let mut immutable = 3;
            immutable = 4;
            nonexistent = 5;
            """);
    }
}
