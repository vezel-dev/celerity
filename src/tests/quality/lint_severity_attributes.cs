public sealed partial class QualityTests
{
    [Fact]
    public Task lint_severity_attributes()
    {
        return TestAsync(
            """
            mod {
                fn code() {
                    0B111;

                    @lint "uppercase-base-indicator:none"
                    {
                        0O111;

                        @lint "uppercase-base-indicator:error"
                        {
                            0X111;
                        };

                        0O111;
                    };

                    0B111;
                }
            }
            """,
            UppercaseBaseIndicatorPass.Instance);
    }
}
