namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("test", HelpText = "Run unit tests in Celerity code.")]
internal sealed class TestVerb : Verb
{
    [Value(0, HelpText = "Source code directory.")]
    public required string? Directory { get; init; }

    public override Task<int> RunAsync()
    {
        // TODO: Implement this.
        return Task.FromResult(0);
    }
}
