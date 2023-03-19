namespace Vezel.Celerity.Language.Tooling.Diagnostics;

public abstract class DiagnosticStyle
{
    // TODO: It would be better to have methods that communicate the semantic meaning of the text being written.

    public abstract ValueTask WriteDecoratedAsync(
        TextWriter writer, string text, bool intense, CancellationToken cancellationToken = default);

    public abstract ValueTask WriteColoredAsync(
        TextWriter writer, string text, Color color, CancellationToken cancellationToken = default);
}
