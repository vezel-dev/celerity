namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("repl", HelpText = "Start an interactive Celerity session.")]
internal sealed class ReplVerb : Verb
{
    [Value(0, HelpText = "Source code directory.")]
    public required string? Directory { get; init; }

    public override Task<int> RunAsync()
    {
        // TODO: Implement this.
        return Task.FromResult(0);
    }
}
