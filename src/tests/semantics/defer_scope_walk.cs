// SPDX-License-Identifier: 0BSD

public sealed partial class SemanticTests
{
    [Fact]
    public Task defer_scope_walk()
    {
        return TestAsync(
            """
            mod {
                err fn main() {
                    defer Foo.bar()?;

                    defer ret 42;

                    defer raise err MyError {};

                    fn() -> {
                        defer this;
                    };

                    while true {
                        defer break;

                        defer next;
                    };
                }
            }
            """);
    }
}
