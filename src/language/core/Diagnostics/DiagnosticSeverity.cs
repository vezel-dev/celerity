// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Diagnostics;

public enum DiagnosticSeverity
{
    None, // Diagnostics that have been suppressed via configuration and/or attributes.
    Warning, // Diagnostics for code that is legal but should be improved.
    Error, // Diagnostics for syntax or semantic errors that prevent execution.
}
