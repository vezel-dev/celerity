// SPDX-License-Identifier: 0BSD

public sealed partial class SemanticTests
{
    [Fact]
    public Task let_for_binding_order()
    {
        return TestAsync(
            """
            mod {
                fn foo() {
                    let x = x;

                    for y in y {
                        y;
                    };
                }
            }
            """);
    }
}
