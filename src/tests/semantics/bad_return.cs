// SPDX-License-Identifier: 0BSD

public sealed partial class SemanticTests
{
    [Fact]
    public Task bad_return()
    {
        return TestAsync(
            """
            mod {
                const a = ret 1;

                const b = fn() -> ret 2;

                fn ret_fn() {
                    fn() -> ret 3;

                    ret 4;
                }
            }
            """);
    }
}
