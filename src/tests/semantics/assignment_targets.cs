public sealed partial class SemanticTests
{
    [Fact]
    public Task assignment_targets()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
                fn assign() {
                    let x = 1;

                    x = 2;

                    let mut y = 3;

                    y = 4;

                    x.field = 5;
                    y.field = 6;

                    "literal" = 7;

                    let arr1 = [1, 2, 3];
                    let arr2 = mut [1, 2, 3];

                    arr1[0] = 8;
                    arr2[0] = 9;
                }
            }
            """);
    }
}
