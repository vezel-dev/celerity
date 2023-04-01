using Vezel.Celerity.Driver.Workspaces;

namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("script", isDefault: true, Hidden = true, HelpText = "Run a Celerity script.")]
internal sealed class ScriptVerb : Verb
{
    [Value(0, Required = true, HelpText = "Entry point file.")]
    public required string File { get; init; }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    protected override ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        var workspace = new ScriptWorkspace(File);

        // TODO: Run the script.
        _ = workspace;

        return ValueTask.FromResult(0);
    }
}
