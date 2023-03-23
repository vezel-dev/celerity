using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Semantics;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Quality;

public sealed class LintContext
{
    public SemanticTree Tree { get; }

    private readonly LintPass _pass;

    private readonly ImmutableArray<Diagnostic>.Builder _diagnostics;

    internal LintContext(
        SemanticTree tree,
        LintPass pass,
        ImmutableArray<Diagnostic>.Builder diagnostics)
    {
        Tree = tree;
        _pass = pass;
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
        Check.Argument(!span.IsEmpty, span);
        Check.NullOrEmpty(message);
        Check.Null(notes);
        Check.All(notes, static note => note.Message != null);

        _diagnostics.Add(
            new(
                Tree.Syntax,
                span,
                _pass.Code,
                _pass.Severity,
                message,
                notes.Select(static t => new DiagnosticNote(t.Span, t.Message)).ToImmutableArray()));
    }
}
