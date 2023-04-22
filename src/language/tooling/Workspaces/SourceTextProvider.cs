namespace Vezel.Celerity.Language.Tooling.Workspaces;

public abstract class SourceTextProvider
{
    protected internal abstract ValueTask<SourceText> GetTextAsync(
        Workspace workspace, string path, CancellationToken cancellationToken);
}
