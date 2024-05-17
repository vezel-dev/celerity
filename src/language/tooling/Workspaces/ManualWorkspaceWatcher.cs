// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Tooling.Workspaces;

public sealed class ManualWorkspaceWatcher : WorkspaceWatcher
{
    // This is intended for use by e.g. a language server that gets fed document events from a language client. It does
    // not perform an initial scan.

    public ManualWorkspaceWatcher(Workspace workspace)
        : base(workspace)
    {
    }

    public new void AddDocument(string path)
    {
        base.AddDocument(path);
    }

    public new void EditDocument(string path)
    {
        base.EditDocument(path);
    }

    public new void MoveDocument(string oldPath, string newPath)
    {
        base.MoveDocument(oldPath, newPath);
    }

    public new void DeleteDocument(string path)
    {
        base.DeleteDocument(path);
    }

    public new void ClearDocuments()
    {
        base.ClearDocuments();
    }
}
