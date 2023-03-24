public sealed partial class QualityTests
{
    [Fact]
    public Task unused_local_symbols()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
                const unused_const = 1;

                fn unused_fn(unused_fn_param) {
                    fn(unused_lambda_param) -> 2;

                    let unused_binding = 3;
                }
            }
            """,
            UnusedLocalSymbolPass.Instance);
    }
}
