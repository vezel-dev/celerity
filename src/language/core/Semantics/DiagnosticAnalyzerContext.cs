// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Semantics;

public sealed class DiagnosticAnalyzerContext
{
    public DocumentSemantics Root { get; }

    private readonly Lock _lock = new();

    private readonly ImmutableArray<Diagnostic>.Builder _diagnostics;

    private bool _invalidated;

    internal DiagnosticAnalyzerContext(DocumentSemantics root, ImmutableArray<Diagnostic>.Builder diagnostics)
    {
        Root = root;
        _diagnostics = diagnostics;
    }

    internal void Invalidate()
    {
        lock (_lock)
            _invalidated = true;
    }

    public void ReportDiagnostic(
        SourceTextSpan span,
        DiagnosticSeverity severity,
        DiagnosticCode code,
        string message,
        params (SourceTextSpan Span, string Message)[] notes)
    {
        ReportDiagnostic(span, severity, code, message, notes.AsEnumerable());
    }

    public void ReportDiagnostic(
        SourceTextSpan span,
        DiagnosticSeverity severity,
        DiagnosticCode code,
        string message,
        IEnumerable<(SourceTextSpan Span, string Message)> notes)
    {
        AddDiagnostic(Diagnostic.Create(Root.Syntax.Tree, span, severity, code, message, notes));
    }

    public void AddDiagnostic(Diagnostic diagnostic)
    {
        Check.Null(diagnostic);
        Check.Argument(diagnostic.Tree == Root.Syntax.Tree, diagnostic);

        lock (_lock)
        {
            Check.Operation(!_invalidated);

            _diagnostics.Add(diagnostic);
        }
    }
}
