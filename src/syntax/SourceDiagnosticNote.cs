namespace Vezel.Celerity.Syntax;

public sealed class SourceDiagnosticNote
{
    public SyntaxItem Item { get; }

    public SourceLocation Location { get; }

    public string Message { get; }

    private SourceDiagnosticNote(SyntaxItem item, SourceLocation location, string message)
    {
        Item = item;
        Location = location;
        Message = message;
    }

    public static SourceDiagnosticNote Create(SyntaxItem item, SourceLocation location, string message)
    {
        ArgumentNullException.ThrowIfNull(item);
        _ = location.FullPath ?? throw new ArgumentException(null, nameof(location));
        ArgumentException.ThrowIfNullOrEmpty(message);

        return new(item, location, message);
    }

    public override string ToString()
    {
        return $"{Location}: Note: {Message}";
    }
}
