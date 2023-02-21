using Vezel.Celerity.Diagnostics;

namespace Vezel.Celerity.Text;

public sealed class SourceDiagnosticNote
{
    public SourceLocation Location { get; }

    public string Message { get; }

    private SourceDiagnosticNote(SourceLocation location, string message)
    {
        Location = location;
        Message = message;
    }

    public static SourceDiagnosticNote Create(SourceLocation location, string message)
    {
        Check.Argument(!location.IsMissing, location);
        Check.NullOrEmpty(message);

        return new(location, message);
    }

    public override string ToString()
    {
        return $"{Location}: note: {Message}";
    }
}
