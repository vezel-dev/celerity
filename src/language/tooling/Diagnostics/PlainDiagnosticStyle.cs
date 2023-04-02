namespace Vezel.Celerity.Language.Tooling.Diagnostics;

public sealed class PlainDiagnosticStyle : DiagnosticStyle
{
    public static PlainDiagnosticStyle Instance { get; } = new();

    private PlainDiagnosticStyle()
    {
    }

    public override ValueTask WriteAsync(
        DiagnosticSeverity? severity,
        DiagnosticPart part,
        string value,
        TextWriter writer,
        CancellationToken cancellationToken = default)
    {
        Check.Enum(severity);
        Check.Range(severity != DiagnosticSeverity.None, severity);
        Check.Enum(part);
        Check.Null(value);
        Check.Null(writer);

        return new(writer.WriteAsync(value.AsMemory(), cancellationToken));
    }
}
