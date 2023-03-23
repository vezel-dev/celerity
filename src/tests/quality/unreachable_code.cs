public sealed partial class QualityTests
{
    [Fact]
    public Task unreachable_code()
    {
        return TestAsync(
            SyntaxMode.Module,
            """
            mod {
                err fn main() {
                    if true {
                        ret nil;

                        "this is dead";
                    };

                    if true {
                        raise err MyError {};

                        "this is also dead";
                    };

                    while true {
                        if true {
                            next;

                            "this too";
                        };

                        if false {
                            break;

                            "and this";
                            "multiple";
                            "statements";
                        };
                    };
                }
            }
            """,
            UnreachableCodePass.Instance);
    }
}
