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

    public override Task<int> ExecuteAsync(CommandContext context, CheckCommandSettings settings)
    {
        // TODO

        return Task.FromResult(1);
    }
}
