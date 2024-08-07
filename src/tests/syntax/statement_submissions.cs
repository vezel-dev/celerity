// SPDX-License-Identifier: 0BSD

public sealed partial class SyntaxTests
{
    [Fact]
    public Task statement_submissions()
    {
        return TestAsync(
            SyntaxMode.Interactive,
            """
            2 + 2;
            assert true;
            let x = 42;
            if true {
                false;
            }
            foo();
            """);
    }
}
