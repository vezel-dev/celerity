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
            var syntax = SyntaxTree.Parse(text, SyntaxMode.Module);
            var semantics = SemanticTree.Analyze(syntax);
            var lint = LintAnalysis.Create(semantics, LintPass.DefaultPasses, LintConfiguration.Default);
            var diags = syntax
                .Diagnostics
                .Concat(semantics.Diagnostics)
                .Concat(lint.Diagnostics)
                .OrderBy(diag => diag.Span)
                .ToArray();

            await DiagnosticPrinter.PrintAsync(text, lint.Diagnostics);

            errors |= diags.Any(diag => diag.IsError);
        }

        return errors ? 1 : 0;
    }
}