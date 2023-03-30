namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("test", HelpText = "Run unit tests in Celerity code.")]
internal sealed class TestVerb : Verb
{
    [Value(0, HelpText = "Source code directory.")]
    public required string? Directory { get; init; }

    public override ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        // TODO: Implement this.
        return ValueTask.FromResult(0);
    }
}
