public sealed partial class DriverTests
{
    [Fact]
    public Task simple_run()
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
            builder => builder.WithArguments("run", "main.cel"),
            0);
    }
}
