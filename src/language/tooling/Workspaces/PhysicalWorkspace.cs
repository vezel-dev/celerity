namespace Vezel.Celerity.Language.Tooling.Workspaces;

public abstract class PhysicalWorkspace : Workspace
{
    public const string EntryPointDocumentName = "main.cel";

    protected PhysicalWorkspace(string path)
        : base(path)
    {
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    protected internal override sealed async ValueTask<SourceText> LoadTextAsync(
        string path, CancellationToken cancellationToken)
    {
        Check.Argument(WorkspaceWatcher.IsValidPath(path), path);

        return new StringSourceText(
            path, await File.ReadAllTextAsync(System.IO.Path.Join(Path, path), cancellationToken)
            .ConfigureAwait(false));
    }
}
