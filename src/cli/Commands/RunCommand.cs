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

    public override Task<int> ExecuteAsync(CommandContext context, RunCommandSettings settings)
    {
        // TODO

        return Task.FromResult(0);
    }
}
