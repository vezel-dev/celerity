// SPDX-License-Identifier: 0BSD

public sealed partial class QualityTests
{
    [Fact]
    public Task tests_without_assert()
    {
        return TestAsync(
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
