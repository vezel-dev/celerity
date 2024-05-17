// SPDX-License-Identifier: 0BSD

[SuppressMessage("", "CA1050")]
public abstract partial class CelerityTests
{
    private protected static async Task VerifyDiagnosticsAsync(
        IEnumerable<Diagnostic> diagnostics, string file, string name)
    {
        var writer = new DiagnosticWriter(
            new DiagnosticWriterConfiguration().WithWidthMeasurer(static rune => MonospaceWidth.Measure(rune) ?? 0));
        await using var text = new StringWriter(CultureInfo.InvariantCulture);

        foreach (var diag in diagnostics)
            if (diag.Severity != DiagnosticSeverity.None)
                await writer.WriteAsync(diag, text);

        _ = await Verifier
            .Verify(text.ToString(), "txt", sourceFile: file)
            .ScrubLinesWithReplace(
                line => DiagnosticPathRegex().Replace(
                    line, m => $"{m.Groups[1].Value}{m.Groups[2].Value.Replace('\\', '/')}{m.Groups[3].Value}"))
            .UseTypeName(name)
            .UseMethodName("diags")
            .ToTask();
    }

    [GeneratedRegex(
        @"^(---*> )(\w+)(.cel \(\d+,\d+\)-\(\d+,\d+\))$", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex DiagnosticPathRegex();
}
