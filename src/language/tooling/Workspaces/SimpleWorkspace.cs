namespace Vezel.Celerity.Language.Tooling.Workspaces;

public sealed class SimpleWorkspace : Workspace
{
    private static readonly IEnumerable<DiagnosticAnalyzer> _diagnosticAnalyzers =
        [
            new LintDiagnosticAnalyzer(LintPass.DefaultPasses, LintConfiguration.Default),
        ];

    private readonly bool _disableAnalysis;

    private SimpleWorkspace(string path, SourceTextProvider textProvider, bool disableAnalysis)
        : base(path, textProvider)
    {
        _disableAnalysis = disableAnalysis;
    }

    public static SimpleWorkspace Open(string path, SourceTextProvider textProvider, bool disableAnalysis = false)
    {
        return new(path, textProvider, disableAnalysis);
    }

    protected internal override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
    {
        return _diagnosticAnalyzers;
    }

    protected override WorkspaceDocumentAttributes GetDocumentAttributes(string path)
    {
        var attrs = path == WorkspaceDocument.EntryPointPath
            ? WorkspaceDocumentAttributes.EntryPoint
            : WorkspaceDocumentAttributes.None;

        if (_disableAnalysis)
            attrs |= WorkspaceDocumentAttributes.DisableAnalyzers | WorkspaceDocumentAttributes.SuppressDiagnostics;

        return attrs;
    }
}
