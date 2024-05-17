// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Driver.Workspaces;

internal sealed class ScriptWorkspace : Workspace
{
    public ScriptWorkspace(string file)
        : base(System.IO.Path.GetDirectoryName(file)!, PhysicalSourceTextProvider.Instance)
    {
        new ManualWorkspaceWatcher(this).AddDocument(System.IO.Path.GetFileName(file));
    }

    protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
    {
        throw new UnreachableException();
    }

    protected override WorkspaceDocumentAttributes GetDocumentAttributes(string path)
    {
        return WorkspaceDocumentAttributes.EntryPoint | WorkspaceDocumentAttributes.DisableAnalyzers;
    }
}
