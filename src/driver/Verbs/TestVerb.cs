namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("test", HelpText = "Run unit tests in Celerity code.")]
internal sealed class TestVerb : Verb
{
    [Value(0, HelpText = "Project directory.")]
    public required string? Directory { get; init; }

    protected override async ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        var workspace = await OpenWorkspaceAsync(Directory, disableAnalysis: false, cancellationToken);

        // TODO: Implement this.
        _ = workspace;

        return 0;
    }
}
