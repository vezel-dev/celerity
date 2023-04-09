using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Diagnostics;

public sealed class DiagnosticNote
{
    public Diagnostic Parent { get; private set; } = null!;

    public SourceTextSpan Span { get; }

    public string Message { get; }

    internal DiagnosticNote(SourceTextSpan span, string message)
    {
        Span = span;
        Message = message;
    }

    internal void SetParent(Diagnostic parent)
    {
        Parent = parent;
    }

    public SourceTextLocation GetLocation()
    {
        return Parent.Tree.GetText().GetLocation(Span);
    }

    public override string ToString()
    {
        return $"{GetLocation()}: Note: {Message}";
    }
}
