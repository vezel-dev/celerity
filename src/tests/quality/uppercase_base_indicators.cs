// SPDX-License-Identifier: 0BSD

public sealed partial class QualityTests
{
    [Fact]
    public Task uppercase_base_indicators()
    {
        return TestAsync(
            """
            mod {
                fn foo() {
                    0b101;
                    0B101;
                    0x101;
                    0X101;
                    0o101;
                    0O101;
                }
            }
            """,
            UppercaseBaseIndicatorPass.Instance);
    }
}
