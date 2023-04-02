namespace Vezel.Celerity.Language.Tooling.Workspaces;

public sealed class SimpleWorkspace : PhysicalWorkspace
{
    private static readonly IEnumerable<DiagnosticAnalyzer> _diagnosticAnalyzers = new[]
    {
        new LintDiagnosticAnalyzer(LintPass.DefaultPasses, LintConfiguration.Default),
    };

    private readonly bool _disableAnalysis;

    private SimpleWorkspace(string path, bool disableAnalysis)
        : base(path)
    {
        _disableAnalysis = disableAnalysis;
    }

    public static SimpleWorkspace Open(string path, bool disableAnalysis = false)
    {
        return new(path, disableAnalysis);
    }

    protected internal override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
    {
        return _diagnosticAnalyzers;
    }

    protected override WorkspaceDocumentAttributes GetDocumentAttributes(string path)
    {
        var attrs = path == EntryPointDocumentName
            ? WorkspaceDocumentAttributes.EntryPoint
            : WorkspaceDocumentAttributes.None;

        if (_disableAnalysis)
            attrs |= WorkspaceDocumentAttributes.DisableAnalyzers | WorkspaceDocumentAttributes.SuppressDiagnostics;

        return attrs;
    }
}
