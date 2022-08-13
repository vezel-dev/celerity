namespace Vezel.Celerity.Syntax;

public sealed class SourceDiagnostic
{
    public SyntaxItem Item { get; }

    public SourceDiagnosticSeverity Severity { get; }

    public bool IsError => Severity == SourceDiagnosticSeverity.Error;

    public SourceLocation Location { get; }

    public string Message { get; }

    public ImmutableArray<SourceDiagnosticNote> Notes { get; }

    public bool HasNotes => !Notes.IsEmpty;

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
        ArgumentNullException.ThrowIfNull(item);
        _ = Enum.IsDefined(severity) ? true : throw new ArgumentOutOfRangeException(nameof(severity));
        _ = location.FullPath ?? throw new ArgumentException(null, nameof(location));
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentNullException.ThrowIfNull(notes);
        _ = notes.All(n => n != null) ? true : throw new ArgumentException(null, nameof(notes));

        return new(item, severity, location, message, notes.ToImmutableArray());
    }

    public override string ToString()
    {
        return $"{Location}: {Severity}: {Message}";
    }
}
