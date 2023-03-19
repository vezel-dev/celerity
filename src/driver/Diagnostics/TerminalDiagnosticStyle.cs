namespace Vezel.Celerity.Driver.Diagnostics;

internal sealed class TerminalDiagnosticStyle : DiagnosticStyle
{
    private readonly bool _interactive;

    public TerminalDiagnosticStyle(TerminalWriter writer)
    {
        _interactive = writer.IsInteractive;
    }

    public override ValueTask WriteDecoratedAsync(
        TextWriter writer, string text, bool intense, CancellationToken cancellationToken = default)
    {
        return WriteAsync(writer, ControlSequences.SetDecorations(intense: intense), text, cancellationToken);
    }

    public override ValueTask WriteColoredAsync(
        TextWriter writer, string text, Color color, CancellationToken cancellationToken = default)
    {
        return WriteAsync(
            writer, ControlSequences.SetForegroundColor(color.R, color.G, color.B), text, cancellationToken);
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    private async ValueTask WriteAsync(
        TextWriter writer, string sequence, string text, CancellationToken cancellationToken)
    {
        if (_interactive)
            await writer.WriteAsync(sequence.AsMemory(), cancellationToken);

        await writer.WriteAsync(text.AsMemory(), cancellationToken);

        if (_interactive)
            await writer.WriteAsync(ControlSequences.ResetAttributes().AsMemory(), cancellationToken);
    }
}
