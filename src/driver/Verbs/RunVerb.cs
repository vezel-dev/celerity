using Vezel.Celerity.Driver.Diagnostics;

namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("run", true, HelpText = "Run a Celerity program.")]
internal sealed class RunVerb : Verb
{
    [Value(0, Required = true, HelpText = "Entry point file.")]
    public required string File { get; init; }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public override async ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        // TODO: Replace all of this.

        var syntax = SyntaxTree.Parse(
            new StringSourceText(File, await System.IO.File.ReadAllTextAsync(File, cancellationToken)),
            SyntaxMode.Module);
        var semantics = SemanticTree.Analyze(syntax, null);

        // SyntaxTree and SemanticTree never emit suppressed diagnostics.
        var diags = syntax.Diagnostics.Concat(semantics.Diagnostics).OrderBy(static diag => diag.Span).ToArray();
        var stderr = Terminal.StandardError;
        var writer = new DiagnosticWriter(new DiagnosticConfiguration().WithStyle(new TerminalDiagnosticStyle(stderr)));

        for (var i = 0; i < diags.Length; i++)
        {
            await writer.WriteAsync(diags[i], stderr.TextWriter, cancellationToken);

            if (i != diags.Length - 1)
                await stderr.WriteLineAsync(cancellationToken);
        }

        return diags.Any(static diag => diag.IsError) ? 1 : 0;
    }
}
