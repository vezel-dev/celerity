namespace Vezel.Celerity.Driver.Commands.Format;

internal abstract class FormatCommand : AsyncCommand<FormatCommand.FormatCommandSettings>
{
    [SuppressMessage("", "CA1812")]
    public sealed class FormatCommandSettings : CommandSettings
    {
        [CommandArgument(0, "[directory]")]
        [Description("Source code directory")]
        public string? Directory { get; init; }
    }
}
