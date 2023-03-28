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
        return TestAsync(default(InteractiveContext), contents, pass, file, name);
    }

    private static Task TestAsync(
        Func<InteractiveContext, InteractiveContext> contextSetup,
        string contents,
        LintPass pass,
        [CallerFilePath] string file = "",
        [CallerMemberName] string name = "")
    {
        return TestAsync(contextSetup(InteractiveContext.Default), contents, pass, file, name);
    }

    private static Task TestAsync(InteractiveContext? context, string contents, LintPass pass, string file, string name)
    {
        var syntax = SyntaxTree.Parse(
            new StringSourceText($"{name}.cel", contents),
            context != null ? SyntaxMode.Interactive : SyntaxMode.Module,
            discardText: true);
        var semantics = SemanticTree.Analyze(
            syntax, context, new LintDiagnosticAnalyzer(new[] { pass }, LintConfiguration.Default));

        // Lint tests should not have syntax and semantic diagnostics, so no need to sort by span here.
        return VerifyDiagnosticsAsync(syntax.Diagnostics.Concat(semantics.Diagnostics), file, name);
    }
}
