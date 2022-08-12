namespace Vezel.Celerity.Driver.Commands;

[SuppressMessage("", "CA1812")]
internal sealed class TestCommand : AsyncCommand<TestCommand.TestCommandSettings>
{
    public sealed class TestCommandSettings : CommandSettings
    {
        [CommandArgument(0, "[directory]")]
        [Description("Source code directory")]
        public string? Directory { get; init; }
    }

    public override Task<int> ExecuteAsync(CommandContext context, TestCommandSettings settings)
    {
        // TODO

        return Task.FromResult(0);
    }
}
