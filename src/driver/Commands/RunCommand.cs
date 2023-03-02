namespace Vezel.Celerity.Driver.Commands;

[SuppressMessage("", "CA1812")]
internal sealed class RunCommand : AsyncCommand<RunCommand.RunCommandSettings>
{
    public sealed class RunCommandSettings : CommandSettings
    {
        [CommandArgument(0, "[file]")]
        [Description("Entry point file")]
        [DefaultValue("main.cel")]
        public string File { get; }

        public RunCommandSettings(string file)
        {
            File = file;
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, RunCommandSettings settings)
    {
        // TODO: Replace all of this.

        var text = new StringSourceText(settings.File, await File.ReadAllTextAsync(settings.File));
        var semantics = SemanticAnalysis.Create(SyntaxAnalysis.Create(text, SyntaxMode.Module));

        DiagnosticPrinter.Print(text, semantics.Diagnostics);

        return semantics.HasErrors ? 1 : 0;
    }
}
