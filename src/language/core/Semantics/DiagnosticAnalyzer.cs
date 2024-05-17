// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Semantics;

public abstract class DiagnosticAnalyzer
{
    protected internal abstract void Analyze(DiagnosticAnalyzerContext context);
}
