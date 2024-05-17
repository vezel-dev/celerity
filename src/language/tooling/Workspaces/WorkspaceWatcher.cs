// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Tooling.Workspaces;

public abstract class WorkspaceWatcher
{
    public Workspace Workspace { get; }

    private readonly object _lock = new();

    protected WorkspaceWatcher(Workspace workspace)
    {
        Workspace = workspace;
    }

    internal static bool IsValidPath(string path)
    {
        Check.NullOrWhiteSpace(path);

        // TODO: It would be good to verify that the path does not contain any . or .. segments.
        return !Path.IsPathFullyQualified(path) && Path.GetExtension(path) == ".cel";
    }

    protected void AddDocument(string path)
    {
        Check.Argument(IsValidPath(path), path);

        lock (_lock)
            Workspace.AddOrEditDocument(path);
    }

    protected void EditDocument(string path)
    {
        Check.Argument(IsValidPath(path), path);

        lock (_lock)
            Workspace.AddOrEditDocument(path);
    }

    protected void MoveDocument(string oldPath, string newPath)
    {
        Check.Argument(IsValidPath(oldPath), oldPath);
        Check.Argument(IsValidPath(newPath), newPath);

        lock (_lock)
            Workspace.MoveDocument(oldPath, newPath);
    }

    protected void DeleteDocument(string path)
    {
        Check.Argument(IsValidPath(path), path);

        lock (_lock)
            Workspace.DeleteDocument(path);
    }

    protected void ClearDocuments()
    {
        lock (_lock)
            Workspace.ClearDocuments();
    }
}
