public sealed partial class SemanticTests
{
    [Fact]
    public Task tail_ret()
    {
        return TestAsync(
            """
            mod {
                fn tail_call() {
                    tail ret tail_call();

                    tail ret tail_call(2 + 2, :foo, "bar");

                    tail ret tail_call() + 42;

                    tail ret Foo.tail_call();

                    tail ret Foo.tail_call(2 + 2, :foo, "bar");

                    tail ret Foo.tail_call() + 42;

                    fn() -> tail ret this();

                    fn() -> tail ret this(2 + 2, :foo, "bar");

                    fn() -> tail ret this() + 42;
                }
            }
            """);
    }
}
