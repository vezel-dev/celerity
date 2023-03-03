using Vezel.Celerity.Language.Diagnostics;

namespace Vezel.Celerity.Language.Quality;

public sealed class LintConfiguration
{
    public static LintConfiguration Default { get; } =
        new(ImmutableDictionary<DiagnosticCode, DiagnosticSeverity?>.Empty);

    private readonly ImmutableDictionary<DiagnosticCode, DiagnosticSeverity?> _severities;

    private LintConfiguration(ImmutableDictionary<DiagnosticCode, DiagnosticSeverity?> severities)
    {
        _severities = severities;
    }

    public bool TryGetSeverity(DiagnosticCode code, out DiagnosticSeverity? severity)
    {
        Check.Argument(code.Code != null, code);

        return _severities.TryGetValue(code, out severity);
    }

    public LintConfiguration WithSeverity(DiagnosticCode code, DiagnosticSeverity? severity)
    {
        Check.Argument(code.Code != null, code);
        Check.Enum(severity);

        return new(_severities.SetItem(code, severity));
    }

    public LintConfiguration WithoutSeverity(DiagnosticCode code)
    {
        Check.Argument(code.Code != null, code);

        return new(_severities.Remove(code));
    }
}
