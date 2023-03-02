namespace Vezel.Celerity.Driver.Commands;

[SuppressMessage("", "CA1812")]
internal sealed class CheckCommand : AsyncCommand<CheckCommand.CheckCommandSettings>
{
    public sealed class CheckCommandSettings : CommandSettings
    {
        [CommandArgument(0, "[directory]")]
        [Description("Source code directory")]
        public string? Directory { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, CheckCommandSettings settings)
    {
        // TODO: Replace all of this.

        var errors = false;

        foreach (var file in Directory.EnumerateFiles(settings.Directory ?? Environment.CurrentDirectory))
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
