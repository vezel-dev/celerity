namespace Vezel.Celerity.Language.Diagnostics;

public enum DiagnosticSeverity
{
    Suggestion, // Diagnostics that are intended to be displayed in an editor only.
    Warning, // Diagnostics for code that is legal but should be improved.
    Error, // Diagnostics for syntax or semantic errors that prevent execution.
}