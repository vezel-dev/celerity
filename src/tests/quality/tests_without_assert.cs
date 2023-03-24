public sealed partial class QualityTests
{
    [Fact]
    public Task tests_without_assert()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
                test foo {
                    1;
                }

                test bar {
                    assert true;
                }
            }
            """,
            TestWithoutAssertPass.Instance);
    }
}
