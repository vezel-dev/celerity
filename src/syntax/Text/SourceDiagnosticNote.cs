using Vezel.Celerity.Syntax.Tree;

namespace Vezel.Celerity.Syntax.Text;

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
        Check.Null(item);
        Check.Argument(!location.IsMissing, location);
        Check.NullOrEmpty(message);

        return new(item, location, message);
    }

    public override string ToString()
    {
        return $"{Location}: Note: {Message}";
    }
}