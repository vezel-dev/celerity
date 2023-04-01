namespace Vezel.Celerity.Driver.Workspaces;

internal sealed class ScriptWorkspace : PhysicalWorkspace
{
    public ScriptWorkspace(string file)
        : base(System.IO.Path.GetDirectoryName(file)!)
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
