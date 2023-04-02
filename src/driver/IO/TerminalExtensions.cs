namespace Vezel.Celerity.Driver.IO;

internal static class TerminalExtensions
{
    public static ValueTask WriteControlAsync(
        this TerminalWriter writer, string sequence, CancellationToken cancellationToken)
    {
        return writer.IsInteractive ? writer.WriteAsync(sequence, cancellationToken) : default;
    }
}
