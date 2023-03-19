[SuppressMessage("", "CA1050")]
[UsesVerify]
public abstract class CelerityTests
{
    private protected static async Task VerifyDiagnosticsAsync(
        IEnumerable<Diagnostic> diagnostics, string file, string name)
    {
        var writer = new DiagnosticWriter(new());
        await using var text = new StringWriter(CultureInfo.InvariantCulture);

        foreach (var diag in diagnostics)
        {
            await writer.WriteAsync(diag, text);
        }

        _ = await Verifier
            .Verify(text.ToString(), sourceFile: file)
            .UseTypeName(name)
            .UseMethodName("diags")
            .ToTask();
    }
}