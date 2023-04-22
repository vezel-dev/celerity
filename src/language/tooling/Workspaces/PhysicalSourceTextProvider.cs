namespace Vezel.Celerity.Language.Tooling.Workspaces;

public sealed class PhysicalSourceTextProvider : SourceTextProvider
{
    public static PhysicalSourceTextProvider Instance { get; } = new();

    private PhysicalSourceTextProvider()
    {
    }

    protected internal override ValueTask<SourceText> GetTextAsync(
        Workspace workspace, string path, CancellationToken cancellationToken)
    {
        Check.Null(workspace);
        Check.Argument(WorkspaceWatcher.IsValidPath(path), path);

        return GetTextAsync();

        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
        async ValueTask<SourceText> GetTextAsync()
        {
            return new StringSourceText(
                path, await File.ReadAllTextAsync(Path.Join(workspace.Path, path), cancellationToken)
                .ConfigureAwait(false));
        }
    }
}
