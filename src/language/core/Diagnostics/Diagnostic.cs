using Vezel.Celerity.Language.Syntax;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Diagnostics;

public sealed class Diagnostic
{
    public SyntaxTree Tree { get; }

    public SourceTextSpan Span { get; }

    public DiagnosticCode Code { get; }

    public DiagnosticSeverity Severity { get; }

    public bool IsError => Severity == DiagnosticSeverity.Error;

    public string Message { get; }

    public ImmutableArray<DiagnosticNote> Notes { get; }

    internal Diagnostic(
        SyntaxTree tree,
        SourceTextSpan span,
        DiagnosticCode code,
        DiagnosticSeverity severity,
        string message,
        ImmutableArray<DiagnosticNote> notes)
    {
        Tree = tree;
        Span = span;
        Code = code;
        Severity = severity;
        Message = message;
        Notes = notes;

        foreach (var note in notes)
            note.SetParent(this);
    }

    public SourceTextLocation GetLocation()
    {
        return Tree.GetText().GetLocation(Span);
    }

    [SuppressMessage("", "CA1308")]
    public override string ToString()
    {
        return $"{GetLocation()}: {Severity.ToString().ToLowerInvariant()}[{Code}]: {Message}";
    }
}
