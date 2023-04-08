namespace Vezel.Celerity.Language.Tooling.Diagnostics;

public abstract class DiagnosticStyle
{
    public abstract ValueTask WriteAsync(
        DiagnosticSeverity? severity,
        DiagnosticPart part,
        string value,
        TextWriter writer,
        CancellationToken cancellationToken = default);

    public virtual ValueTask WriteLineAsync(TextWriter writer, CancellationToken cancellationToken = default)
    {
        Check.Null(writer);

        return new(writer.WriteLineAsync(default(ReadOnlyMemory<char>), cancellationToken));
    }
}
