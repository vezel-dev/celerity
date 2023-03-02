namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("check", HelpText = "Perform semantic and quality analyses on Celerity code.")]
internal sealed class CheckVerb : Verb
{
    [Value(0, HelpText = "Source code directory.")]
    public required string? Directory { get; init; }

    public override async ValueTask<int> RunAsync()
    {
        // TODO: Replace all of this.

        var directory = Directory ?? Environment.CurrentDirectory;
        var errors = false;

        foreach (var file in System.IO.Directory.EnumerateFiles(directory, "*.cel", SearchOption.AllDirectories))
        {
            var text = new StringSourceText(Path.GetRelativePath(directory, file), await File.ReadAllTextAsync(file));
            var lint = LintAnalysis.Create(
                SemanticAnalysis.Create(
                    SyntaxAnalysis.Create(text, SyntaxMode.Module)),
                LintPass.DefaultPasses,
                LintConfiguration.Default);

            await DiagnosticPrinter.PrintAsync(text, lint.Diagnostics);

            errors |= lint.HasErrors;
        }

        return errors ? 1 : 0;
    }
}
