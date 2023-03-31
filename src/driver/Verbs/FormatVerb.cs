namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("format", HelpText = "Check or fix Celerity code formatting.")]
internal sealed class FormatVerb : Verb
{
    [Value(0, HelpText = "Workspace directory.")]
    public required string? Directory { get; init; }

    [Option('f', "fix", HelpText = "Enable automatic fixing.")]
    public required bool Fix { get; init; }

    protected override async ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        var workspace = await OpenWorkspaceAsync(Directory, disableAnalysis: true, cancellationToken);

        // TODO: Implement this.
        _ = workspace;

        return 0;
    }
}
