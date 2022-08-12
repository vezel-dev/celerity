namespace Vezel.Celerity.Driver.Commands.Format;

[SuppressMessage("", "CA1812")]
internal sealed class FormatFixCommand : FormatCommand
{
    public override Task<int> ExecuteAsync(CommandContext context, FormatCommandSettings settings)
    {
        // TODO

        return Task.FromResult(0);
    }
}
