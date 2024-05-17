// SPDX-License-Identifier: 0BSD

[SuppressMessage("", "CA1050")]
[SuppressMessage("", "CA1707")]
[SuppressMessage("", "IDE1006")]
[SuppressMessage("", "SA1300")]
[SuppressMessage("", "VSTHRD200")]
public sealed partial class QualityTests : CelerityTests
{
    private static Task TestAsync(
        string contents, LintPass pass, [CallerFilePath] string file = "", [CallerMemberName] string name = "")
    {
        var syntax = SyntaxTree.Parse(
            new StringSourceText($"{name}.cel", contents), SyntaxMode.Module, discardText: true);
        var semantics = SemanticTree.Analyze(
            syntax, context: null, new LintDiagnosticAnalyzer([pass], LintConfiguration.Default));

        return VerifyDiagnosticsAsync(
            syntax.Diagnostics.Concat(semantics.Diagnostics).OrderBy(static diag => diag.Span), file, name);
    }
}
