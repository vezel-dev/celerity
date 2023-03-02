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

        var text = new StringSourceText(File, await System.IO.File.ReadAllTextAsync(File));
        var semantics = SemanticAnalysis.Create(SyntaxAnalysis.Create(text, SyntaxMode.Module));

        await DiagnosticPrinter.PrintAsync(text, semantics.Diagnostics);

        return semantics.HasErrors ? 1 : 0;
    }
}
