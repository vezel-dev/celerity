using Vezel.Celerity.Driver.Diagnostics;

namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("check", HelpText = "Perform semantic and quality analyses on Celerity code.")]
internal sealed class CheckVerb : Verb
{
    [Value(0, HelpText = "Source code directory.")]
    public required string? Directory { get; init; }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public override async ValueTask<int> RunAsync()
    {
        // TODO: Replace all of this.

        var directory = Directory ?? Environment.CurrentDirectory;
        var errors = false;

        foreach (var file in System.IO.Directory
            .EnumerateFiles(directory, "*.cel", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            var syntax = SyntaxTree.Parse(
                new StringSourceText(Path.GetRelativePath(directory, file), await File.ReadAllTextAsync(file)),
                SyntaxMode.Module);
            var semantics = SemanticTree.Analyze(
                syntax, null, new LintDiagnosticAnalyzer(LintPass.DefaultPasses, LintConfiguration.Default));

            var diags = syntax.Diagnostics
                .Concat(semantics.Diagnostics)
                .Where(static diag => diag.Severity != DiagnosticSeverity.None)
                .OrderBy(static diag => diag.Span)
                .ToArray();
            var stderr = Terminal.StandardError;
            var writer = new DiagnosticWriter(
                new DiagnosticConfiguration().WithStyle(new TerminalDiagnosticStyle(stderr)));

            for (var i = 0; i < diags.Length; i++)
            {
                await writer.WriteAsync(diags[i], stderr.TextWriter);

                if (i != diags.Length - 1)
                    await stderr.WriteLineAsync();
            }

            errors |= diags.Any(static diag => diag.IsError);
        }

        return errors ? 1 : 0;
    }
}
