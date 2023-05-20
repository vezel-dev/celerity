namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("run", HelpText = "Run a Celerity program.")]
internal sealed class RunVerb : Verb
{
    [Value(0, HelpText = "Program arguments.")]
    public required IEnumerable<string> Arguments { get; init; }

    [Option('w', "workspace", HelpText = "Set workspace directory.")]
    public required string? Workspace { get; init; }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    protected override async ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        if (Workspace != null && string.IsNullOrWhiteSpace(Workspace))
            throw new DriverException($"Invalid workspace path '{Workspace}'.");

        var workspace = await OpenWorkspaceAsync(Workspace, disableAnalysis: false, cancellationToken);

        if (workspace is ProjectWorkspace { Configuration.Kind: not ProjectKind.Program })
            throw new DriverException(
                $"Workspace not configured as program in '{ProjectConfiguration.DefaultFileName}'.");

        if (workspace.GetEntryPointDocument() is not { })
            throw new DriverException(
                $"No entry point document named '{WorkspaceDocument.EntryPointPath}' found in the workspace.");

        // TODO: Run the program.

        return 0;
    }
}
