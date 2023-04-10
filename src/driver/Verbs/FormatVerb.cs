namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("format", HelpText = "Check or fix Celerity code formatting.")]
internal sealed class FormatVerb : Verb
{
    [Option('w', "workspace", HelpText = "Set workspace directory.")]
    public required string? Workspace { get; init; }

    [Option('f', "fix", HelpText = "Enable automatic fixing.")]
    public required bool Fix { get; init; }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    protected override async ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        if (Workspace != null && string.IsNullOrWhiteSpace(Workspace))
            throw new DriverException($"Invalid workspace path '{Workspace}'.");

        var workspace = await OpenWorkspaceAsync(Workspace, disableAnalysis: true, cancellationToken);

        // TODO: Implement this.
        _ = workspace;

        return 0;
    }
}
