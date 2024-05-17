// SPDX-License-Identifier: 0BSD

public sealed partial class SemanticTests
{
    [Fact]
    public Task test_reference()
    {
        return TestAsync(
            """
            mod {
                test foo { assert true; }

                fn bar() { foo; }
            }
            """);
    }
}
