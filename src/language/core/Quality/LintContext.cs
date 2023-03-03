using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Syntax;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Quality;

[SuppressMessage("", "CA1815")]
public readonly struct LintContext
{
    private readonly SyntaxTree _tree;

    private readonly DiagnosticCode _code;

    private readonly DiagnosticSeverity _severity;

    private readonly ImmutableArray<Diagnostic>.Builder _diagnostics;

    internal LintContext(
        SyntaxTree tree,
        DiagnosticCode code,
        DiagnosticSeverity severity,
        ImmutableArray<Diagnostic>.Builder diagnostics)
    {
        _tree = tree;
        _code = code;
        _severity = severity;
        _diagnostics = diagnostics;
    }

    public void ReportDiagnostic(
        SourceTextSpan span, string message, params (SourceTextSpan Span, string Message)[] notes)
    {
        ReportDiagnostic(span, message, notes.AsEnumerable());
    }

    public void ReportDiagnostic(
        SourceTextSpan span, string message, IEnumerable<(SourceTextSpan Span, string Message)> notes)
    {
        Check.NullOrEmpty(message);
        Check.Null(notes);
        Check.All(notes, static note => note.Message != null);

        _diagnostics.Add(
            new(
                _tree,
                span,
                _code,
                _severity,
                message,
                notes.Select(t => new DiagnosticNote(t.Span, t.Message)).ToImmutableArray()));
    }
}
