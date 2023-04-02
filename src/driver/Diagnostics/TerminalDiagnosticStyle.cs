namespace Vezel.Celerity.Driver.Diagnostics;

internal sealed class TerminalDiagnosticStyle : DiagnosticStyle
{
    private static readonly Color _warningColor = Color.FromArgb(255, 255, 0);

    private static readonly Color _errorColor = Color.FromArgb(255, 0, 0);

    private static readonly Color _noteColor = Color.FromArgb(0, 255, 255);

    private static readonly Color _separatorColor = Color.FromArgb(125, 125, 125);

    private static readonly Color _locationColor = Color.FromArgb(100, 175, 225);

    private static readonly Color _spanColor = Color.FromArgb(100, 125, 225);

    private static readonly Color _marginColor = Color.FromArgb(175, 175, 175);

    private readonly bool _interactive;

    public TerminalDiagnosticStyle(TerminalWriter writer)
    {
        _interactive = writer.IsInteractive;
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public override async ValueTask WriteAsync(
        DiagnosticSeverity? severity,
        DiagnosticPart part,
        string value,
        TextWriter writer,
        CancellationToken cancellationToken = default)
    {
        var sequence = part switch
        {
            _ when !_interactive => null,
            DiagnosticPart.Severity or DiagnosticPart.Caret => severity switch
            {
                DiagnosticSeverity.Warning =>
                    ControlSequences.SetForegroundColor(_warningColor.R, _warningColor.G, _warningColor.B),
                DiagnosticSeverity.Error =>
                    ControlSequences.SetForegroundColor(_errorColor.R, _errorColor.G, _errorColor.B),
                null => ControlSequences.SetForegroundColor(_noteColor.R, _noteColor.G, _noteColor.B),
                _ => throw new UnreachableException(),
            },
            DiagnosticPart.Separator =>
                ControlSequences.SetForegroundColor(_separatorColor.R, _separatorColor.G, _separatorColor.B),
            DiagnosticPart.Path =>
                ControlSequences.SetForegroundColor(_locationColor.R, _locationColor.G, _locationColor.B),
            DiagnosticPart.Range => ControlSequences.SetForegroundColor(_spanColor.R, _spanColor.G, _spanColor.B),
            DiagnosticPart.Margin =>
                ControlSequences.SetForegroundColor(_marginColor.R, _marginColor.G, _marginColor.B),
            DiagnosticPart.TargetLine => ControlSequences.SetDecorations(intense: true),
            _ => null,
        };

        if (sequence != null)
            await writer.WriteAsync(sequence.AsMemory(), cancellationToken);

        await writer.WriteAsync(value.AsMemory(), cancellationToken);

        if (sequence != null)
            await writer.WriteAsync(ControlSequences.ResetAttributes().AsMemory(), cancellationToken);
    }
}
