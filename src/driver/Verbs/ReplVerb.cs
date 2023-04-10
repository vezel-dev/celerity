namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("repl", HelpText = "Start an interactive Celerity session.")]
internal sealed class ReplVerb : Verb
{
    [Option('w', "workspace", HelpText = "Set workspace directory.")]
    public required string? Workspace { get; init; }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    protected override async ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        if (Workspace != null && string.IsNullOrWhiteSpace(Workspace))
            throw new DriverException($"Invalid workspace path '{Workspace}'.");

        if (!In.IsInteractive)
            throw new DriverException("The REPL can only be run in an interactive terminal.");

        var workspace = await OpenWorkspaceAsync(Workspace, disableAnalysis: false, cancellationToken);

        // TODO: Implement this.
        _ = workspace;

        return 0;
    }
}
