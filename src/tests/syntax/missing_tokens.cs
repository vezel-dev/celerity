// SPDX-License-Identifier: 0BSD

public sealed partial class SyntaxTests
{
    [Fact]
    public Task missing_tokens()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
                type x = -;

                @foo
                fn () { 1; }

                fn bar() {
                    (if true { rec { baz : 42 }; }).baz;
                    if true { rec { baz : 42 }; }.baz;
                }
            }
            """);
    }
}
