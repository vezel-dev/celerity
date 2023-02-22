namespace Vezel.Celerity.Quality;

[SuppressMessage("", "CA1815")]
public readonly struct LintContext
{
    private readonly SourceDiagnosticCode _code;

    private readonly SourceDiagnosticSeverity _severity;

    private readonly ImmutableArray<SourceDiagnostic>.Builder _diagnostics;

    internal LintContext(
        SourceDiagnosticCode code,
        SourceDiagnosticSeverity severity,
        ImmutableArray<SourceDiagnostic>.Builder diagnostics)
    {
        _code = code;
        _severity = severity;
        _diagnostics = diagnostics;
    }

    public void CreateDiagnostic(
        SourceLocation location, string message, params (SourceLocation Location, string Message)[] notes)
    {
        CreateDiagnostic(location, message, notes.AsEnumerable());
    }

    public void CreateDiagnostic(
        SourceLocation location, string message, IEnumerable<(SourceLocation Location, string Message)> notes)
    {
        _diagnostics.Add(
            SourceDiagnostic.Create(
                _code,
                _severity,
                location,
                message,
                notes.Select(t => SourceDiagnosticNote.Create(t.Location, t.Message))));
    }
}
