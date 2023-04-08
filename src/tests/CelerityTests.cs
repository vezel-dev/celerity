[SuppressMessage("", "CA1050")]
[UsesVerify]
public abstract class CelerityTests
{
    private protected static async Task VerifyDiagnosticsAsync(
        IEnumerable<Diagnostic> diagnostics, string file, string name)
    {
        var writer = new DiagnosticWriter(
            new DiagnosticConfiguration().WithWidthMeasurer(static rune => MonospaceWidth.Measure(rune) ?? 0));
        await using var text = new StringWriter(CultureInfo.InvariantCulture);

        foreach (var diag in diagnostics)
            if (diag.Severity != DiagnosticSeverity.None)
                await writer.WriteAsync(diag, text);

        _ = await Verifier
            .Verify(text.ToString(), "txt", sourceFile: file)
            .UseTypeName(name)
            .UseMethodName("diags")
            .ToTask();
    }
}
