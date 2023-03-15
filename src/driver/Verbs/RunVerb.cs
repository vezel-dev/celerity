namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("run", true, HelpText = "Run a Celerity program.")]
internal sealed class RunVerb : Verb
{
    [Value(0, Required = true, HelpText = "Entry point file.")]
    public required string File { get; init; }

    public override async ValueTask<int> RunAsync()
    {
        // TODO: Replace all of this.

        var syntax = SyntaxTree.Parse(
            new StringSourceText(File, await System.IO.File.ReadAllTextAsync(File)), SyntaxMode.Module);
        var semantics = SemanticTree.Analyze(syntax);
        var diags = syntax.Diagnostics.Concat(semantics.Diagnostics).ToArray();

        await DiagnosticPrinter.PrintAsync(diags);

        return diags.Any(static diag => diag.IsError) ? 1 : 0;
    }
}
