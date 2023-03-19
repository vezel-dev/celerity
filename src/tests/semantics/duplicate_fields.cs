public sealed partial class SemanticTests
{
    [Fact]
    public Task duplicate_fields()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
                fn foo() {
                    rec {
                        x : 1,
                        x : 2,
                    };
                }

                fn bar() {
                    err MyError {
                        y : 3,
                        y : 4,
                    };
                }

                fn baz(value) {
                    match value {
                        rec { x : _a, x : _b } -> 5,
                        err { y : _c, y : _d } -> 6,
                    };
                }
            }
            """);
    }
}
