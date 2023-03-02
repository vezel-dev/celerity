namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("check", HelpText = "Perform semantic and quality analyses on Celerity code.")]
internal sealed class CheckVerb : Verb
{
    [Value(0, HelpText = "Source code directory.")]
    public required string? Directory { get; init; }

    public override async Task<int> RunAsync()
    {
        // TODO: Replace all of this.

        var errors = false;

        foreach (var file in System.IO.Directory.EnumerateFiles(
            Directory ?? Environment.CurrentDirectory, "*.cel", SearchOption.AllDirectories))
        {
            var text = new StringSourceText(file, await File.ReadAllTextAsync(file));
            var lint = LintAnalysis.Create(
                SemanticAnalysis.Create(
                    SyntaxAnalysis.Create(text, SyntaxMode.Module)),
                LintPass.DefaultPasses,
                LintConfiguration.Default);

            DiagnosticPrinter.Print(text, lint.Diagnostics);

            errors |= lint.HasErrors;
        }

        return errors ? 1 : 0;
    }
}
