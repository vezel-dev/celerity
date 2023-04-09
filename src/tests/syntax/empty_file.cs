public sealed partial class SyntaxTests
{
    [Fact]
    public Task empty_file()
    {
        return TestAsync(
            SyntaxMode.Module,
            string.Empty);
    }
}
