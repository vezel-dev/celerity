// SPDX-License-Identifier: 0BSD

[SuppressMessage("", "CA1050")]
[SuppressMessage("", "CA1707")]
[SuppressMessage("", "IDE1006")]
[SuppressMessage("", "SA1300")]
[SuppressMessage("", "VSTHRD200")]
public sealed partial class SyntaxTests : CelerityTests
{
    private static Task TestAsync(
        SyntaxMode mode, string contents, [CallerFilePath] string file = "", [CallerMemberName] string name = "")
    {
        var path = $"{name}.cel";
        var syntax = SyntaxTree.Parse(new StringSourceText(path, contents), mode, discardText: true);

        syntax.Path.ShouldBe(path);
        syntax.Root.ToFullString().ShouldBe(contents);

        return VerifyDiagnosticsAsync(syntax.Diagnostics, file, name);
    }
}
