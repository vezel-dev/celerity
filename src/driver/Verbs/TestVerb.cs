namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("test", HelpText = "Run unit tests in Celerity code.")]
internal sealed class TestVerb : Verb
{
    [Option('w', "workspace", HelpText = "Set workspace directory.")]
    public required string? Workspace { get; init; }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    protected override async ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        if (Workspace != null && string.IsNullOrWhiteSpace(Workspace))
            throw new DriverException($"Invalid workspace path '{Workspace}'.");

        var workspace = await OpenWorkspaceAsync(Workspace, disableAnalysis: false, cancellationToken);

        // TODO: Implement this.
        _ = workspace;

        return 0;
    }
}
