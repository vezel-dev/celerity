namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("serve", HelpText = "Host the Celerity language server.")]
internal sealed class ServeVerb : Verb
{
    [Value(0, HelpText = "Project directory.")]
    public required string? Directory { get; init; }

    protected override ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        // TODO: Implement this.
        return ValueTask.FromResult(0);
    }
}
