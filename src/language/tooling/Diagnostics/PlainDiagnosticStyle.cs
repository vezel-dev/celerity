namespace Vezel.Celerity.Language.Tooling.Diagnostics;

public sealed class PlainDiagnosticStyle : DiagnosticStyle
{
    public static PlainDiagnosticStyle Instance { get; } = new();

    private PlainDiagnosticStyle()
    {
    }

    public override ValueTask WriteDecoratedAsync(
        TextWriter writer, string text, bool intense, CancellationToken cancellationToken = default)
    {
        return WriteAsync(writer, text, cancellationToken);
    }

    public override ValueTask WriteColoredAsync(
        TextWriter writer, string text, Color color, CancellationToken cancellationToken = default)
    {
        Check.Argument(!color.IsEmpty && color.A == byte.MaxValue, color);

        return WriteAsync(writer, text, cancellationToken);
    }

    private static ValueTask WriteAsync(TextWriter writer, string text, CancellationToken cancellationToken)
    {
        Check.Null(writer);
        Check.Null(text);

        return WriteAsync();

        async ValueTask WriteAsync()
        {
            await writer.WriteAsync(text.AsMemory(), cancellationToken).ConfigureAwait(false);
        }
    }
}
