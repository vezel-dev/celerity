namespace Vezel.Celerity.Syntax;

public sealed class SourceDiagnostic
{
    public SyntaxItem Item { get; }

    public SourceDiagnosticSeverity Severity { get; }

    public bool IsError => Severity == SourceDiagnosticSeverity.Error;

    public SourceLocation Location { get; }

    public string Message { get; }

    public ImmutableArray<SourceDiagnosticNote> Notes { get; }

    private SourceDiagnostic(
        SyntaxItem item,
        SourceDiagnosticSeverity severity,
        SourceLocation location,
        string message,
        ImmutableArray<SourceDiagnosticNote> notes)
    {
        Item = item;
        Severity = severity;
        Location = location;
        Message = message;
        Notes = notes;
    }

    public static SourceDiagnostic Create(
        SyntaxItem item,
        SourceDiagnosticSeverity severity,
        SourceLocation location,
        string message,
        params SourceDiagnosticNote[] notes)
    {
        return Create(item, severity, location, message, notes.AsEnumerable());
    }

    public static SourceDiagnostic Create(
        SyntaxItem item,
        SourceDiagnosticSeverity severity,
        SourceLocation location,
        string message,
        IEnumerable<SourceDiagnosticNote> notes)
    {
        Check.Null(item);
        Check.Enum(severity);
        Check.Argument(!location.IsMissing, location);
        Check.NullOrEmpty(message);
        Check.Null(notes);
        Check.All(notes, static note => note != null);

        return new(item, severity, location, message, notes.ToImmutableArray());
    }

    public override string ToString()
    {
        return $"{Location}: {Severity}: {Message}";
    }
}
