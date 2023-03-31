public sealed partial class QualityTests
{
    [Fact]
    public Task unreachable_catch()
    {
        return TestAsync(
            """
            mod {
                fn main() {
                    try catch { _ -> 1 };

                    try catch {};

                    try 42 catch { _ -> 2 };

                    try raise nil catch { _ -> 3 };

                    try Foo.bar()? catch { _ -> 4 };
                }
            }
            """,
            UnreachableCodePass.Instance);
    }
}
