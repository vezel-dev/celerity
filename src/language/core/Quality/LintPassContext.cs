// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Quality;

public sealed class LintPassContext
{
    public DocumentSemantics Root { get; }

    private readonly object _lock = new();

    private readonly LintPass _pass;

    private readonly List<Diagnostic> _diagnostics;

    private bool _invalidated;

    internal LintPassContext(DocumentSemantics root, LintPass pass, List<Diagnostic> diagnostics)
    {
        Root = root;
        _pass = pass;
        _diagnostics = diagnostics;
    }

    internal void Invalidate()
    {
        lock (_lock)
            _invalidated = true;
    }

    public void ReportDiagnostic(
        SourceTextSpan span, string message, params (SourceTextSpan Span, string Message)[] notes)
    {
        ReportDiagnostic(span, message, notes.AsEnumerable());
    }

    public void ReportDiagnostic(
        SourceTextSpan span, string message, IEnumerable<(SourceTextSpan Span, string Message)> notes)
    {
        AddDiagnostic(
            Diagnostic.Create(Root.Syntax.Tree, span, DiagnosticSeverity.Warning, _pass.Code, message, notes));
    }

    public void AddDiagnostic(Diagnostic diagnostic)
    {
        Check.Null(diagnostic);
        Check.Argument(
            (diagnostic.Tree, diagnostic.Severity, diagnostic.Code) ==
            (Root.Syntax.Tree, DiagnosticSeverity.Warning, _pass.Code),
            diagnostic);

        lock (_lock)
        {
            Check.Operation(!_invalidated);

            _diagnostics.Add(diagnostic);
        }
    }
}
