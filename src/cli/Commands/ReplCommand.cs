namespace Vezel.Celerity.Driver.Commands;

[SuppressMessage("", "CA1812")]
internal sealed class ReplCommand : AsyncCommand<ReplCommand.ReplCommandSettings>
{
    public sealed class ReplCommandSettings : CommandSettings
    {
        [CommandArgument(0, "[directory]")]
        [Description("Source code directory")]
        public string? Directory { get; init; }
    }

    public override Task<int> ExecuteAsync(CommandContext context, ReplCommandSettings settings)
    {
        // TODO

        return Task.FromResult(1);
    }
}
