namespace Vezel.Celerity.Driver;

internal static class DiagnosticPrinter
{
    // TODO: Move all of this to Vezel.Celerity.Language.Tooling.

    private const int ContextLines = 3;

    private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    private static readonly bool _interactive = Terminal.TerminalOut.IsInteractive;

    private static async ValueTask WriteControlAsync(string sequence)
    {
        if (_interactive)
            await Terminal.ErrorAsync(sequence);
    }

    public static async ValueTask PrintAsync(IEnumerable<Diagnostic> diagnostics)
    {
        async ValueTask PrintWindowAsync(
            IReadOnlyList<(int Line, string Text)> lines,
            int margin,
            SourceTextLocation location,
            string severity,
            (byte R, byte G, byte B) color,
            string message)
        {
            async ValueTask PrintContextAsync(IEnumerable<(int Line, string Text)> lines)
            {
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

            await WriteControlAsync(ControlSequences.SetForegroundColor(color.R, color.G, color.B));
            await Terminal.ErrorAsync($"{severity}: ");
            await WriteControlAsync(ControlSequences.ResetAttributes());
            await Terminal.ErrorLineAsync(message);

            var start = location.Start;
            var end = location.End;

            var startLine = start.Line + 1;
            var endLine = end.Line + 1;

            await WriteControlAsync(ControlSequences.SetForegroundColor(128, 128, 128));
            await Terminal.ErrorAsync($"{new string('-', margin + 1)}> ");
            await WriteControlAsync(ControlSequences.SetForegroundColor(100, 175, 225));
            await Terminal.ErrorLineAsync(
                $"{location.Path} ({startLine},{start.Character + 1})-({endLine},{end.Character + 1})");
            await WriteControlAsync(ControlSequences.ResetAttributes());

            await PrintContextAsync(lines.Where(t => t.Line >= startLine - ContextLines && t.Line < startLine));

            foreach (var (line, text) in lines.Where(t => t.Line >= startLine && t.Line <= endLine))
            {
                await PrintLineAsync(line, text, ControlSequences.SetDecorations(intense: true));

                // Edge case: If a diagnostic points to the new-line character on line A, its SourceTextLocation.End
                // will point to the first character of the following line B. This causes line B to be included in this
                // loop. But since no characters on that line are actually affected, we print no caret. This causes the
                // edge case logic below to kick in, resulting in a nonsensical caret at the end of line B.
                //
                // Deal with this by avoiding printing the caret line.
                if (line == endLine && end.Character == 0)
                    continue;

                var sb = new StringBuilder();

                var isStart = line == startLine;
                var isEnd = line == endLine;

                var visible = false;

                for (var i = 0; i < text.Length; i++)
                {
                    var gtStart = i >= start.Character;
                    var ltEnd = i < end.Character;

                    var ch = (isStart, isEnd, gtStart, ltEnd) switch
                    {
                        (false, false, _, _) or
                        (true, true, true, true) or
                        (true, false, true, _) or
                        (false, true, _, true) => '^',
                        _ => ' ',
                    };

                    if (ch == '^')
                        visible = true;

                    _ = sb.Append(ch);
                }

                // Edge case: In various situations, a diagnostic can point to lines in such a way that no visible
                // character on that line gets a caret under it. For example, consider a line that consists of nothing
                // but the CR/LF sequence; we have stripped that earlier so it will not be processed in this loop.
                // Another case is a file that ends abruptly when a token was expected; there, the diagnostic will point
                // just past the end of the source text.
                //
                // The fix is straightforward: If a line is affected by a diagnostic but has not had a caret printed
                // yet, we just print one past the end of the line. It will point to a white space character that we
                // just pretend exists. (This is how most text editors seem to handle this case as well.)
                if (!visible)
                    _ = sb.Append('^');

                await WriteControlAsync(ControlSequences.SetForegroundColor(128, 128, 128));
                await Terminal.ErrorAsync($"{new string(' ', margin)} : ");
                await WriteControlAsync(ControlSequences.SetForegroundColor(color.R, color.G, color.B));
                await Terminal.ErrorLineAsync(sb.ToString());
                await WriteControlAsync(ControlSequences.ResetAttributes());
            }

            await PrintContextAsync(lines.Where(t => t.Line > endLine && t.Line <= endLine + ContextLines));
        }

        foreach (var diag in diagnostics)
        {
            var lines = diag
                .Tree
                .GetText()
                .Lines
                .Select(static line => (Line: line.Line + 1, Text: line.ToString().ReplaceLineEndings(string.Empty)))
                .ToArray();
            var margin = lines[^1].Line.ToString(_culture).Length;

            await PrintWindowAsync(
                lines,
                margin,
                diag.GetLocation(),
                $"{diag.Severity}[{diag.Code}]",
                diag.Severity switch
                {
                    DiagnosticSeverity.Suggestion => (0, 225, 0),
                    DiagnosticSeverity.Warning => (225, 225, 0),
                    DiagnosticSeverity.Error => (225, 0, 0),
                    _ => throw new UnreachableException(),
                },
                diag.Message);

            foreach (var note in diag.Notes)
                await PrintWindowAsync(lines, margin, note.GetLocation(), "Note", (0, 225, 225), note.Message);

            await Terminal.ErrorLineAsync();
        }
    }
}
