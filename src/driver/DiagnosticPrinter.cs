namespace Vezel.Celerity.Driver;

internal static class DiagnosticPrinter
{
    // TODO: Move all of this to Vezel.Celerity.Language.Tooling.

    private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    private static readonly bool _interactive = Terminal.TerminalOut.IsInteractive;

    private static async ValueTask WriteControlAsync(string sequence)
    {
        if (_interactive)
            await Terminal.ErrorAsync(sequence);
    }

    public static async ValueTask PrintAsync(SourceText text, ImmutableArray<SourceDiagnostic> diagnostics)
    {
        var lines = text
            .Lines
            .Select(line => (Line: line.Line + 1, Text: line.ToString().ReplaceLineEndings(string.Empty)))
            .ToArray();
        var margin = lines[^1].Line.ToString(_culture).Length;

        async ValueTask PrintWindowAsync(
            SourceLocation location, string severity, (byte R, byte G, byte B) color, string message)
        {
            async ValueTask PrintContextAsync(IReadOnlyList<(int Line, string Text)> lines)
            {
                if (!lines.All(t => string.IsNullOrWhiteSpace(t.Text)))
                    foreach (var (line, text) in lines)
                        await PrintLineAsync(line, text, null);
            }

            async ValueTask PrintLineAsync(int line, string text, string? sequence)
            {
                await WriteControlAsync(ControlSequences.SetForegroundColor(100, 175, 225));
                await Terminal.ErrorAsync(line.ToString(_culture).PadLeft(margin));
                await WriteControlAsync(ControlSequences.SetForegroundColor(128, 128, 128));
                await Terminal.ErrorAsync(" | ");
                await WriteControlAsync(ControlSequences.ResetAttributes());

                if (sequence != null)
                    await WriteControlAsync(sequence);

                await Terminal.ErrorLineAsync(text);

                if (sequence != null)
                    await WriteControlAsync(ControlSequences.ResetAttributes());
            }

            var start = location.Start;
            var end = location.End;

            var startLine = start.Line + 1;
            var endLine = end.Line + 1;

            const int Context = 3;

            var leading = lines.Where(t => t.Line >= startLine - Context && t.Line < startLine).ToArray();
            var affected = lines.Where(t => t.Line >= startLine && t.Line <= endLine).ToArray();
            var trailing = lines.Where(t => t.Line > endLine && t.Line <= endLine + Context).ToArray();

            await WriteControlAsync(ControlSequences.SetForegroundColor(color.R, color.G, color.B));
            await Terminal.ErrorAsync($"{severity}: ");
            await WriteControlAsync(ControlSequences.ResetAttributes());
            await Terminal.ErrorLineAsync(message);

            await WriteControlAsync(ControlSequences.SetForegroundColor(128, 128, 128));
            await Terminal.ErrorAsync($"{new string('-', margin + 1)}> ");
            await WriteControlAsync(ControlSequences.SetForegroundColor(100, 175, 225));
            await Terminal.ErrorLineAsync(
                $"{location.Path} ({startLine},{start.Character + 1})-({endLine},{end.Character + 1})");
            await WriteControlAsync(ControlSequences.ResetAttributes());

            await PrintContextAsync(leading);

            foreach (var (line, text) in affected)
            {
                await PrintLineAsync(line, text, ControlSequences.SetDecorations(intense: true));

                var sb = new StringBuilder();

                var isStart = line == startLine;
                var isEnd = line == endLine;

                for (var i = 0; i < text.Length; i++)
                {
                    var gtStart = i >= start.Character;
                    var ltEnd = i < end.Character;

                    _ = sb.Append((isStart, isEnd, gtStart, ltEnd) switch
                    {
                        (false, false, _, _) or
                        (true, true, true, true) or
                        (true, false, true, _) or
                        (false, true, _, true) => '^',
                        _ => ' ',
                    });
                }

                await WriteControlAsync(ControlSequences.SetForegroundColor(128, 128, 128));
                await Terminal.ErrorAsync($"{new string(' ', margin)} : ");
                await WriteControlAsync(ControlSequences.SetForegroundColor(color.R, color.G, color.B));
                await Terminal.ErrorLineAsync(sb.ToString());
                await WriteControlAsync(ControlSequences.ResetAttributes());
            }

            await PrintContextAsync(trailing);
        }

        foreach (var diag in diagnostics)
        {
            await PrintWindowAsync(
                diag.Location,
                $"{diag.Severity}[{diag.Code}]",
                diag.Severity switch
                {
                    SourceDiagnosticSeverity.Suggestion => (0, 225, 0),
                    SourceDiagnosticSeverity.Warning => (225, 225, 0),
                    SourceDiagnosticSeverity.Error => (225, 0, 0),
                    _ => throw new UnreachableException(),
                },
                diag.Message);

            foreach (var note in diag.Notes)
                await PrintWindowAsync(note.Location, "Note", (0, 225, 225), note.Message);

            await Terminal.ErrorLineAsync();
        }
    }
}
