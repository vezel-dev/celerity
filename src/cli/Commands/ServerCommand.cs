namespace Vezel.Celerity.Driver.Commands;

[SuppressMessage("", "CA1812")]
internal sealed class ServerCommand : AsyncCommand<ServerCommand.ServerCommandSettings>
{
    public sealed class ServerCommandSettings : CommandSettings
    {
        [CommandArgument(0, "[directory]")]
        [Description("Source code directory")]
        public string? Directory { get; init; }
    }

    public override Task<int> ExecuteAsync(CommandContext context, ServerCommandSettings settings)
    {
        // TODO

        return Task.FromResult(1);
    }
}
