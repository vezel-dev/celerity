namespace Vezel.Celerity.Language.Tooling.Diagnostics;

public sealed class DiagnosticWriter
{
    public DiagnosticWriterConfiguration Configuration { get; }

    // Keep in sync with TextFacts.
    private static readonly ReadOnlyMemory<string> _newLines = new[]
    {
        "\n",
        "\r",
        "\u0085",
        "\u2028",
        "\u2029",
    };

    public DiagnosticWriter(DiagnosticWriterConfiguration configuration)
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
            var lines = diagnostic
                .Tree
                .GetText()
                .Lines
                .Select(
                    static line =>
                    {
                        // Most editors and tools do not process VT and FF.
                        var sb = new StringBuilder(line.ToString())
                            .Replace('\v', ' ')
                            .Replace('\f', ' ');

                        foreach (var ch in _newLines.Span)
                            _ = sb.Replace(ch, string.Empty);

                        return (Line: line.Line + 1, sb.ToString());
                    })
                .ToArray();
            var margin = lines[^1].Line.ToString(writer.FormatProvider).Length;
            var style = Configuration.Style;
            var measurer = Configuration.WidthMeasurer;

            [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
            async ValueTask WriteWindowAsync(
                IReadOnlyList<(int Line, string Text)> lines,
                SourceTextLocation location,
                DiagnosticSeverity? severity,
                string severityValue,
                string messageValue)
            {
                [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
                async ValueTask WriteContextAsync(IEnumerable<(int Line, string Text)> lines)
                {
                    foreach (var (line, text) in lines)
                        await WriteContextOrTargetAsync(line, text, DiagnosticPart.ContextLine).ConfigureAwait(false);
                }

                ValueTask WriteAsync(DiagnosticPart part, string value)
                {
                    return style.WriteAsync(severity, part, value, writer, cancellationToken);
                }

                ValueTask WriteLineAsync()
                {
                    return style.WriteLineAsync(writer, cancellationToken);
                }

                var tab = new string(' ', Configuration.TabWidth);

                [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
                async ValueTask WriteContextOrTargetAsync(int line, string text, DiagnosticPart part)
                {
                    var lineValue = line.ToString(writer.FormatProvider);

                    await WriteAsync(DiagnosticPart.WhiteSpace, new(' ', margin - lineValue.Length))
                        .ConfigureAwait(false);
                    await WriteAsync(DiagnosticPart.Margin, lineValue).ConfigureAwait(false);
                    await WriteAsync(DiagnosticPart.WhiteSpace, " ").ConfigureAwait(false);
                    await WriteAsync(DiagnosticPart.Separator, "|").ConfigureAwait(false);
                    await WriteAsync(DiagnosticPart.WhiteSpace, " ").ConfigureAwait(false);
                    await WriteAsync(part, text.Replace("\t", tab, StringComparison.Ordinal)).ConfigureAwait(false);
                    await WriteLineAsync().ConfigureAwait(false);
                }

                await WriteAsync(DiagnosticPart.Severity, $"{severityValue}:").ConfigureAwait(false);
                await WriteAsync(DiagnosticPart.WhiteSpace, " ").ConfigureAwait(false);
                await WriteAsync(DiagnosticPart.Message, messageValue).ConfigureAwait(false);
                await WriteLineAsync().ConfigureAwait(false);

                var start = location.Start;
                var end = location.End;

                var startLine = start.Line + 1;
                var endLine = end.Line + 1;

                await WriteAsync(DiagnosticPart.Separator, $"{new('-', margin + 1)}>").ConfigureAwait(false);
                await WriteAsync(DiagnosticPart.WhiteSpace, " ").ConfigureAwait(false);
                await WriteAsync(DiagnosticPart.Path, location.Path).ConfigureAwait(false);
                await WriteAsync(DiagnosticPart.WhiteSpace, " ").ConfigureAwait(false);
                await WriteAsync(
                    DiagnosticPart.Range, $"({startLine},{start.Character + 1})-({endLine},{end.Character + 1})")
                    .ConfigureAwait(false);
                await WriteLineAsync().ConfigureAwait(false);

                var context = Configuration.ContextLines;

                await WriteContextAsync(lines.Where(t => t.Line >= startLine - context && t.Line < startLine))
                    .ConfigureAwait(false);

                foreach (var (line, text) in lines.Where(t => t.Line >= startLine && t.Line <= endLine))
                {
                    // Edge case: If a diagnostic points to the new-line character on line A, its SourceTextLocation.End
                    // will point to the first character of the following line B. This causes line B to be included in
                    // this loop. But since no characters on that line are actually affected, we do not write a caret.
                    // This causes the caret edge case logic below to kick in, resulting in a nonsensical caret at the
                    // end of line B.
                    //
                    // Detect this case here and print this line as a normal context line if so.
                    var skip = line == endLine && start.Line + 1 != line && end.Character == 0;

                    await WriteContextOrTargetAsync(
                        line, text, skip ? DiagnosticPart.ContextLine : DiagnosticPart.TargetLine)
                        .ConfigureAwait(false);

                    if (skip)
                        continue;

                    var blanks = 0;
                    var carets = 0;

                    var isStart = line == startLine;
                    var isEnd = line == endLine;

                    for (var i = 0; i < text.Length;)
                    {
                        var isCaret = (isStart, isEnd, i >= start.Character, i < end.Character) switch
                        {
                            (false, false, _, _) or
                            (true, true, true, true) or
                            (true, false, true, _) or
                            (false, true, _, true) => true,
                            _ => false,
                        };

                        var (rune, count) = text[i] == '\t'
                            ? (new('\t'), tab.Length)
                            : Rune.TryGetRuneAt(text, i, out var r)
                                ? (r, measurer(r))
                                : (Rune.ReplacementChar, 1);

                        Check.Operation(count >= 0);

                        if (!isCaret)
                        {
                            // If we finished writing the caret(s), avoid writing trailing white space.
                            if (carets != 0)
                                break;

                            blanks += count;
                        }
                        else
                            carets += count;

                        i += rune.Utf16SequenceLength;
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
                    if (carets == 0)
                        carets++;

                    await WriteAsync(DiagnosticPart.WhiteSpace, new(' ', margin + 1)).ConfigureAwait(false);
                    await WriteAsync(DiagnosticPart.Separator, ":").ConfigureAwait(false);
                    await WriteAsync(DiagnosticPart.WhiteSpace, " ").ConfigureAwait(false);

                    if (blanks != 0)
                        await WriteAsync(DiagnosticPart.WhiteSpace, new(' ', blanks)).ConfigureAwait(false);

                    await WriteAsync(DiagnosticPart.Caret, new('^', carets)).ConfigureAwait(false);
                    await WriteLineAsync().ConfigureAwait(false);
                }

                await WriteContextAsync(lines.Where(t => t.Line > endLine && t.Line <= endLine + context))
                    .ConfigureAwait(false);
            }

            await WriteWindowAsync(
                lines,
                diagnostic.GetLocation(),
                diagnostic.Severity,
                $"{diagnostic.Severity}[{diagnostic.Code}]",
                diagnostic.Message)
                .ConfigureAwait(false);

            foreach (var note in diagnostic.Notes)
                await WriteWindowAsync(lines, note.GetLocation(), null, "Note", note.Message).ConfigureAwait(false);
        }
    }
}
