[SuppressMessage("", "CA1050")]
[SuppressMessage("", "CA1707")]
[SuppressMessage("", "IDE1006")]
[SuppressMessage("", "SA1300")]
[SuppressMessage("", "VSTHRD200")]
public sealed partial class SemanticTests : CelerityTests
{
    private static Task TestAsync(
        SyntaxMode mode, string text, [CallerFilePath] string file = "", [CallerMemberName] string name = "")
    {
        var syntax = SyntaxTree.Parse(new StringSourceText($"{name}.cel", text), mode, discardText: true);
        var semantics = SemanticTree.Analyze(syntax);

        return VerifyDiagnosticsAsync(syntax.Diagnostics.Concat(semantics.Diagnostics), file, name);
    }
}
