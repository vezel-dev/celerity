public sealed partial class QualityTests
{
    [Fact]
    public Task unreachable_code_submission()
    {
        return TestAsync(
            ctx => ctx,
            """
            {
                ret nil;
                42;
            };
            """,
            UnreachableCodePass.Instance);
    }
}
