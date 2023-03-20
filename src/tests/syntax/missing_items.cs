public sealed partial class SyntaxTests
{
    [Fact]
    public Task missing_items()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
                type tx = ;
                type ty =
                    ;

                const x = ;
                const y =
                    ;

                fn foo() { }
                fn bar() {
                }

                fn baz() {
                    let = 1;
                    let
                        = 2;

                    let _ as = 3;
                    let _ as
                        = 4;
                }
            }
            """);
    }
}
