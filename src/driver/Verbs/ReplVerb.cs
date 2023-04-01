namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("repl", HelpText = "Start an interactive Celerity session.")]
internal sealed class ReplVerb : Verb
{
    [Value(0, HelpText = "Workspace directory.")]
    public required string? Directory { get; init; }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    protected override async ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        if (Directory != null && string.IsNullOrWhiteSpace(Directory))
            throw new DriverException($"Invalid workspace path '{Directory}'.");

        if (!In.IsInteractive)
            throw new DriverException("The REPL can only be run in an interactive terminal.");

        var workspace = await OpenWorkspaceAsync(Directory, disableAnalysis: false, cancellationToken);

        // TODO: Implement this.
        _ = workspace;

        return 0;
    }
}
