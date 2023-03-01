namespace Vezel.Celerity.Driver.Commands;

[SuppressMessage("", "CA1812")]
internal sealed class RunCommand : AsyncCommand<RunCommand.RunCommandSettings>
{
    public sealed class RunCommandSettings : CommandSettings
    {
        [CommandArgument(0, "[file]")]
        [Description("Entry point file")]
        [DefaultValue("main.cel")]
        public string File { get; }

        public RunCommandSettings(string file)
        {
            File = file;
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, RunCommandSettings settings)
    {
        var culture = CultureInfo.InvariantCulture;

        // TODO: Replace all of this.

        var text = new StringSourceText(settings.File, await File.ReadAllTextAsync(settings.File));
        var syntax = SyntaxAnalysis.Create(text, SyntaxMode.Module);
        var semantics = SemanticAnalysis.Create(syntax);
        var lint = LintAnalysis.Create(semantics, LintPass.DefaultPasses, LintConfiguration.Default);

        var lines = text
            .Lines
            .Select(line => (Line: line.Line + 1, Text: line.ToString().ReplaceLineEndings(string.Empty)))
            .ToArray();
        var margin = lines[^1].Line.ToString(culture).Length;

        void PrintWindow(SourceLocation location, string severity, string color, string message)
        {
            void PrintContext(IReadOnlyList<(int Line, string Text)> lines)
            {
                if (!lines.All(t => string.IsNullOrWhiteSpace(t.Text)))
                    foreach (var (line, text) in lines)
                        PrintLine(line, text, null);
            }

            void PrintLine(int line, string text, string? style)
            {
                AnsiConsole.Markup(culture, $"[blue]{{0, {margin}}}[/][grey] | [/]", line);

                if (style != null)
                    AnsiConsole.MarkupLine(culture, $"[{style}]{{0}}[/]", text.EscapeMarkup());
                else
                    AnsiConsole.WriteLine(text);
            }

            var start = location.Start;
            var end = location.End;

            var startLine = start.Line + 1;
            var endLine = end.Line + 1;

            const int Context = 3;

            var leading = lines.Where(t => t.Line >= startLine - Context && t.Line < startLine).ToArray();
            var targets = lines.Where(t => t.Line >= startLine && t.Line <= endLine).ToArray();
            var trailing = lines.Where(t => t.Line > endLine && t.Line <= endLine + Context).ToArray();

            AnsiConsole.MarkupLine(
                culture, $"[{color}]{{0}}:[/] {{1}}", severity.EscapeMarkup(), message.EscapeMarkup());
            AnsiConsole.MarkupLine(
                culture,
                "[grey]{0}>[/] [blue]{1} ({2},{3})-({4},{5})[/]",
                new string('-', margin + 1),
                location.Path!.EscapeMarkup(),
                startLine,
                start.Character + 1,
                endLine,
                end.Character + 1);

            PrintContext(leading);

            foreach (var target in targets)
            {
                PrintLine(target.Line, target.Text, "bold");

                var sb = new StringBuilder();

                var isStart = target.Line == startLine;
                var isEnd = target.Line == endLine;

                for (var i = 0; i < target.Text.Length; i++)
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

                AnsiConsole.MarkupLine(
                    culture,
                    $"[grey]{{0, {margin}}} : [/][{color}]{{1}}[/]",
                    string.Empty,
                    sb.ToString().EscapeMarkup());
            }

            PrintContext(trailing);
        }

        foreach (var diag in lint.Diagnostics)
        {
            PrintWindow(
                diag.Location,
                $"{diag.Severity}[{diag.Code}]",
                diag.Severity switch
                {
                    SourceDiagnosticSeverity.Suggestion => "green",
                    SourceDiagnosticSeverity.Warning => "yellow",
                    SourceDiagnosticSeverity.Error => "red",
                    _ => throw new UnreachableException(),
                },
                diag.Message);

            foreach (var note in diag.Notes)
                PrintWindow(note.Location, "Note", "aqua", note.Message);

            AnsiConsole.WriteLine();
        }

        return semantics.HasErrors ? 1 : 0;
    }
}
