public sealed partial class DriverTests
{
    [Fact]
    public Task simple_script()
    {
        return TestAsync(
            builder => builder
                .AddFile(
                    "main.cel",
                    """
                    mod {
                        fn main(_env) {
                            0;
                        }
                    }
                    """),
            builder => builder.WithArguments("main.cel"),
            0);
    }
}
