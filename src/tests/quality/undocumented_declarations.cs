// SPDX-License-Identifier: 0BSD

public sealed partial class QualityTests
{
    [Fact]
    public Task undocumented_declarations()
    {
        return TestAsync(
            """
            @doc "my fancy module"
            mod {
                pub fn foo() {
                    "this should be documented";
                }

                @doc "my fancy function"
                pub fn bar() {
                    "this is documented";
                }

                @doc false
                pub fn baz() {
                    "this is hidden";
                }
            }
            """,
            UndocumentedPublicSymbolPass.Instance);
    }
}
