namespace Vezel.Celerity.Language.Tooling.Diagnostics;

public sealed class DiagnosticWriter
{
    public DiagnosticConfiguration Configuration { get; }

    private static readonly Color _warningColor = Color.FromArgb(225, 225, 0);

    private static readonly Color _errorColor = Color.FromArgb(225, 0, 0);

    private static readonly Color _noteColor = Color.FromArgb(0, 225, 225);

    private static readonly Color _locationColor = Color.FromArgb(100, 175, 225);

    private static readonly Color _marginColor = Color.FromArgb(100, 175, 225);

    private static readonly Color _barColor = Color.FromArgb(128, 128, 128);

    public DiagnosticWriter(DiagnosticConfiguration configuration)
    {
        Check.Null(configuration);

        Configuration = configuration;
    }

    public ValueTask WriteAsync(Diagnostic diagnostic, TextWriter writer, CancellationToken cancellationToken = default)
    {
        Check.Null(diagnostic);
        Check.Argument(diagnostic.Severity != DiagnosticSeverity.None, diagnostic);
        Check.Null(writer);

        return WriteAsyncCore();

        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
        async ValueTask WriteAsyncCore()
        {
            [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
            async ValueTask WriteWindowAsync(
                IReadOnlyList<(int Line, string Text)> lines,
                int margin,
                SourceTextLocation location,
                string severity,
                Color color,
                string message)
            {
                [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
                async ValueTask WriteContextAsync(IEnumerable<(int Line, string Text)> lines)
                {
                    foreach (var (line, text) in lines)
                        await WriteLineAsync(line, text, false).ConfigureAwait(false);
                }

                var style = Configuration.Style;

                [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
                async ValueTask WriteLineAsync(int line, string text, bool intense)
                {
                    await style.WriteColoredAsync(
                        writer, line.ToString(writer.FormatProvider).PadLeft(margin), _marginColor, cancellationToken)
                        .ConfigureAwait(false);
                    await style.WriteColoredAsync(writer, " | ", _barColor, cancellationToken).ConfigureAwait(false);
                    await style.WriteDecoratedAsync(writer, text, intense, cancellationToken).ConfigureAwait(false);
                    await writer.WriteLineAsync(ReadOnlyMemory<char>.Empty, cancellationToken).ConfigureAwait(false);
                }

                await style.WriteColoredAsync(writer, $"{severity}: ", color, cancellationToken).ConfigureAwait(false);
                await writer.WriteLineAsync(message.AsMemory(), cancellationToken).ConfigureAwait(false);

                var start = location.Start;
                var end = location.End;

                var startLine = start.Line + 1;
                var endLine = end.Line + 1;

                await style.WriteColoredAsync(writer, $"{new string('-', margin + 1)}> ", _barColor, cancellationToken)
                    .ConfigureAwait(false);
                await style.WriteColoredAsync(
                    writer,
                    $"{location.Path} ({startLine},{start.Character + 1})-({endLine},{end.Character + 1})",
                    _locationColor,
                    cancellationToken)
                    .ConfigureAwait(false);
                await writer.WriteLineAsync(ReadOnlyMemory<char>.Empty, cancellationToken).ConfigureAwait(false);

                var context = Configuration.ContextLines;

                await WriteContextAsync(lines.Where(t => t.Line >= startLine - context && t.Line < startLine))
                    .ConfigureAwait(false);

                foreach (var (line, text) in lines.Where(t => t.Line >= startLine && t.Line <= endLine))
                {
                    // Edge case: If a diagnostic points to the new-line character on line A, its SourceTextLocation.End
                    // will point to the first character of the following line B. This causes line B to be included in
                    // this loop. But since no characters on that line are actually affected, we do not write a caret.
                    // This causes the edge case logic below to kick in, resulting in a nonsensical caret at the end of
                    // line B.
                    //
                    // Deal with this by avoiding writing the caret line.
                    var skip = line == endLine && end.Character == 0;

                    await WriteLineAsync(line, text, !skip).ConfigureAwait(false);

                    if (skip)
                        continue;

                    // Using a StringBuilder is more efficient than async calls for each character.
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

                        // If we finished writing the caret(s), avoid writing trailing white space.
                        if (ch == ' ' && visible)
                            break;

                        if (ch == '^')
                            visible = true;

                        _ = sb.Append(ch);
                    }

                    // Edge case: In various situations, a diagnostic can point to lines in such a way that no visible
                    // character on that line gets a caret under it. For example, consider a line that consists of
                    // nothing but the CR/LF sequence; we have stripped that earlier so it will not be processed in this
                    // loop. Another case is a file that ends abruptly when a token was expected; there, the diagnostic
                    // will point just past the end of the source text.
                    //
                    // The fix is straightforward: If a line is affected by a diagnostic but has not had a caret written
                    // yet, we just write one past the end of the line. It will point to a white space character that we
                    // just pretend exists. (This is how most text editors seem to handle this case as well.)
                    if (!visible)
                        _ = sb.Append('^');

                    await style.WriteColoredAsync(writer, $"{new string(' ', margin)} : ", _barColor, cancellationToken)
                        .ConfigureAwait(false);
                    await style.WriteColoredAsync(writer, sb.ToString(), color, cancellationToken)
                        .ConfigureAwait(false);
                    await writer.WriteLineAsync(ReadOnlyMemory<char>.Empty, cancellationToken).ConfigureAwait(false);
                }

                await WriteContextAsync(lines.Where(t => t.Line > endLine && t.Line <= endLine + context))
                    .ConfigureAwait(false);
            }

            var lines = diagnostic
                .Tree
                .GetText()
                .Lines
                .Select(static line => (Line: line.Line + 1, Text: line.ToString().ReplaceLineEndings(string.Empty)))
                .ToArray();
            var margin = lines[^1].Line.ToString(writer.FormatProvider).Length;

            await WriteWindowAsync(
                lines,
                margin,
                diagnostic.GetLocation(),
                $"{diagnostic.Severity}[{diagnostic.Code}]",
                diagnostic.Severity switch
                {
                    DiagnosticSeverity.Warning => _warningColor,
                    DiagnosticSeverity.Error => _errorColor,
                    _ => throw new UnreachableException(),
                },
                diagnostic.Message)
                .ConfigureAwait(false);

            foreach (var note in diagnostic.Notes)
                await WriteWindowAsync(lines, margin, note.GetLocation(), "Note", _noteColor, note.Message)
                    .ConfigureAwait(false);
        }
    }
}
