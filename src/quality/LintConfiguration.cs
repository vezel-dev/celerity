namespace Vezel.Celerity.Quality;

public sealed class LintConfiguration
{
    private readonly ImmutableDictionary<SourceDiagnosticCode, SourceDiagnosticSeverity?> _severities;

    public LintConfiguration()
    {
        _severities = ImmutableDictionary<SourceDiagnosticCode, SourceDiagnosticSeverity?>.Empty;
    }

    private LintConfiguration(ImmutableDictionary<SourceDiagnosticCode, SourceDiagnosticSeverity?> severities)
    {
        _severities = severities;
    }

    public bool TryGetSeverity(SourceDiagnosticCode code, out SourceDiagnosticSeverity? severity)
    {
        Check.Argument(code.Code != null, code);

        return _severities.TryGetValue(code, out severity);
    }

    public LintConfiguration WithSeverity(SourceDiagnosticCode code, SourceDiagnosticSeverity? severity)
    {
        Check.Argument(code.Code != null, code);
        Check.Enum(severity);

        return new(_severities.SetItem(code, severity));
    }

    public LintConfiguration WithoutSeverity(SourceDiagnosticCode code)
    {
        Check.Argument(code.Code != null, code);

        return new(_severities.Remove(code));
    }
}
