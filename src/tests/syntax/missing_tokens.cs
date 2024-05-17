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
            }
            """);
    }
}
