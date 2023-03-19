[SuppressMessage("", "CA1050")]
[SuppressMessage("", "CA1707")]
[SuppressMessage("", "IDE1006")]
[SuppressMessage("", "SA1300")]
[SuppressMessage("", "VSTHRD200")]
public sealed partial class QualityTests : CelerityTests
{
    private static Task TestAsync(
        SyntaxMode mode,
        string text,
        LintPass pass,
        [CallerFilePath] string file = "",
        [CallerMemberName] string name = "")
    {
        var syntax = SyntaxTree.Parse(new StringSourceText($"{name}.cel", text), mode, discardText: true);
        var semantics = SemanticTree.Analyze(syntax);
        var analysis = LintAnalysis.Create(semantics, new[] { pass }, LintConfiguration.Default);

        return VerifyDiagnosticsAsync(
            syntax.Diagnostics.Concat(semantics.Diagnostics).Concat(analysis.Diagnostics), file, name);
    }
}
