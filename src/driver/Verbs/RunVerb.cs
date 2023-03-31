namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("run", HelpText = "Run a Celerity program.")]
internal sealed class RunVerb : Verb
{
    [Value(0, HelpText = "Workspace directory.")]
    public required string? Directory { get; init; }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    protected override async ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        var workspace = await OpenWorkspaceAsync(Directory, disableAnalysis: false, cancellationToken);

        if (workspace.GetEntryPointDocument() is not { } doc)
            throw new DriverException(
                $"No entry point document ('{PhysicalWorkspace.EntryPointDocumentName}') found in the workspace.");

        // TODO: Run the program.

        return 0;
    }
}
