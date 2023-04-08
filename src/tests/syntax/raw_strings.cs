public sealed partial class SyntaxTests
{
    [Fact]
    public Task raw_strings()
    {
        return TestAsync(
            SyntaxMode.Module,
            """"
            mod {
                fn raw_strings() {
                    let a = """asdf
                    let b = """asdf""";
                    let c = """
                    asdf
                    ghjk
                    """;
                    let d = """
                    asdf
                }
            }
            """");
    }
}
