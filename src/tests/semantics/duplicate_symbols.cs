public sealed partial class SemanticTests
{
    [Fact]
    public Task duplicate_symbols()
    {
        return TestAsync(
            """
            mod {
                use Aaa = Bbb;
                use Aaa = Bbb;

                fn foo() { 1; }

                const foo = 2;

                test foo { assert true; }

                fn bar(aaa, aaa) {
                    fn(bbb, bbb) -> 3;
                }

                fn baz() {
                    let (x, x) = (4, 5);
                    let (_x, _x) = (x, x);
                }
            }
            """);
    }
}
