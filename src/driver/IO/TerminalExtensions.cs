namespace Vezel.Celerity.Driver.IO;

internal static class TerminalExtensions
{
    public static void WriteControl(
        this TerminalWriter writer, string sequence)
    {
        if (writer.IsInteractive)
            writer.Write(sequence);
    }

    public static ValueTask WriteControlAsync(
        this TerminalWriter writer, string sequence, CancellationToken cancellationToken)
    {
        return writer.IsInteractive ? writer.WriteAsync(sequence, cancellationToken) : default;
    }
}
