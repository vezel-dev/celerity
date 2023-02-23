namespace Vezel.Celerity.Language.Text;

public sealed class SourceDiagnostic
{
    public SourceDiagnosticCode Code { get; }

    public SourceDiagnosticSeverity Severity { get; }

    public bool IsError => Severity == SourceDiagnosticSeverity.Error;

    public SourceLocation Location { get; }

    public string Message { get; }

    public ImmutableArray<SourceDiagnosticNote> Notes { get; }

    private SourceDiagnostic(
        SourceDiagnosticCode code,
        SourceDiagnosticSeverity severity,
        SourceLocation location,
        string message,
        ImmutableArray<SourceDiagnosticNote> notes)
    {
        Code = code;
        Severity = severity;
        Location = location;
        Message = message;
        Notes = notes;
    }

    public static SourceDiagnostic Create(
        SourceDiagnosticCode code,
        SourceDiagnosticSeverity severity,
        SourceLocation location,
        string message,
        params SourceDiagnosticNote[] notes)
    {
        return Create(code, severity, location, message, notes.AsEnumerable());
    }

    public static SourceDiagnostic Create(
        SourceDiagnosticCode code,
        SourceDiagnosticSeverity severity,
        SourceLocation location,
        string message,
        IEnumerable<SourceDiagnosticNote> notes)
    {
        Check.Argument(code.Code != null, code);
        Check.Enum(severity);
        Check.Argument(!location.IsMissing, location);
        Check.NullOrEmpty(message);
        Check.Null(notes);
        Check.All(notes, static note => note != null);

        return new(code, severity, location, message, notes.ToImmutableArray());
    }

    [SuppressMessage("", "CA1308")]
    public override string ToString()
    {
        return $"{Location}: {Severity.ToString().ToLowerInvariant()}[{Code}]: {Message}";
    }
}
