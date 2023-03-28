[SuppressMessage("", "CA1050")]
[SuppressMessage("", "CA1707")]
[SuppressMessage("", "IDE1006")]
[SuppressMessage("", "SA1300")]
[SuppressMessage("", "VSTHRD200")]
public sealed partial class SemanticTests : CelerityTests
{
    private static Task TestAsync(
        string contents, [CallerFilePath] string file = "", [CallerMemberName] string name = "")
    {
        return TestAsync(default(InteractiveContext), contents, file, name);
    }

    private static Task TestAsync(
        Func<InteractiveContext, InteractiveContext> contextSetup,
        string contents,
        [CallerFilePath] string file = "",
        [CallerMemberName] string name = "")
    {
        return TestAsync(contextSetup(InteractiveContext.Default), contents, file, name);
    }

    private static Task TestAsync(InteractiveContext? context, string contents, string file, string name)
    {
        var syntax = SyntaxTree.Parse(
            new StringSourceText($"{name}.cel", contents),
            context != null ? SyntaxMode.Interactive : SyntaxMode.Module,
            discardText: true);
        var semantics = SemanticTree.Analyze(syntax, context);

        // Semantic tests should not have syntax diagnostics, so no need to sort by span here.
        return VerifyDiagnosticsAsync(syntax.Diagnostics.Concat(semantics.Diagnostics), file, name);
    }
}
