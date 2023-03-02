namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("format", HelpText = "Check or fix Celerity code formatting.")]
internal sealed class FormatVerb : Verb
{
    [Value(0, HelpText = "Source code directory.")]
    public required string? Directory { get; init; }

    [Option('f', "fix", HelpText = "Enable automatic fixing.")]
    public required bool Fix { get; init; }

    public override Task<int> RunAsync()
    {
        // TODO: Implement this.
        return Task.FromResult(0);
    }
}
