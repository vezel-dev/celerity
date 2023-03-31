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
            syntax, null, new LintDiagnosticAnalyzer(new[] { pass }, LintConfiguration.Default));

        // Lint tests should not have syntax and semantic diagnostics, so no need to sort by span here.
        return VerifyDiagnosticsAsync(syntax.Diagnostics.Concat(semantics.Diagnostics), file, name);
    }
}
