using Vezel.Celerity.Language.Syntax;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Diagnostics;

public sealed class Diagnostic
{
    public SyntaxTree Tree { get; }

    public SourceTextSpan Span { get; }

    public DiagnosticSeverity Severity { get; }

    public DiagnosticCode Code { get; }

    public bool IsError => Severity == DiagnosticSeverity.Error;

    public string Message { get; }

    public ImmutableArray<DiagnosticNote> Notes { get; }

    internal Diagnostic(
        SyntaxTree tree,
        SourceTextSpan span,
        DiagnosticSeverity severity,
        DiagnosticCode code,
        string message,
        ImmutableArray<DiagnosticNote> notes)
    {
        Tree = tree;
        Span = span;
        Severity = severity;
        Code = code;
        Message = message;
        Notes = notes;

        foreach (var note in notes)
            note.SetParent(this);
    }

    public static Diagnostic Create(
        SyntaxTree tree,
        SourceTextSpan span,
        DiagnosticSeverity severity,
        DiagnosticCode code,
        string message,
        params (SourceTextSpan Span, string Message)[] notes)
    {
        return Create(tree, span, severity, code, message, notes.AsEnumerable());
    }

    public static Diagnostic Create(
        SyntaxTree tree,
        SourceTextSpan span,
        DiagnosticSeverity severity,
        DiagnosticCode code,
        string message,
        IEnumerable<(SourceTextSpan Span, string Message)> notes)
    {
        Check.Null(tree);
        Check.Argument(tree.Root.FullSpan.Contains(span), span);
        Check.Enum(severity);
        Check.Argument(code is { Code: { }, IsStandard: false }, code);
        Check.NullOrEmpty(message);
        Check.Null(notes);
        Check.All(
            notes,
            tree.Root.FullSpan,
            static (note, fullSpan) =>
                !note.Span.IsEmpty && fullSpan.Contains(note.Span) && !string.IsNullOrEmpty(note.Message));

        return new(
            tree,
            span,
            severity,
            code,
            message,
            notes.Select(static t => new DiagnosticNote(t.Span, t.Message)).ToImmutableArray());
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
