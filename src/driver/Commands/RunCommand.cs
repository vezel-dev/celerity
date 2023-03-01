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

        var lines = text
            .Lines
            .Select(line => (Line: line.Line, Text: line.ToString().ReplaceLineEndings(string.Empty)))
            .ToArray();
        var margin = lines[^1].Line.ToString(culture).Length;

        foreach (var diag in semantics.Diagnostics)
        {
            void PrintContext(IReadOnlyList<(int Line, string Text)> lines)
            {
                if (!lines.All(t => string.IsNullOrWhiteSpace(t.Text)))
                    foreach (var (line, text) in lines)
                        PrintLine(line, text, null);
            }

            void PrintLine(int line, string text, string? style)
            {
                AnsiConsole.Markup(culture, $"[blue]{{0, {margin}}}[/]", line);
                AnsiConsole.Markup("[grey] | [/]");

                if (style != null)
                    AnsiConsole.MarkupLine(culture, $"[{style}]{{0}}[/]", text.EscapeMarkup());
                else
                    AnsiConsole.WriteLine(text);
            }

            const int Context = 3;

            var location = diag.Location;
            var start = location.Start;
            var end = location.End;

            var leading = lines.Where(t => t.Line >= start.Line - Context && t.Line < start.Line).ToArray();
            var targets = lines.Where(t => t.Line >= start.Line && t.Line <= end.Line).ToArray();
            var trailing = lines.Where(t => t.Line > end.Line && t.Line <= end.Line + Context).ToArray();

            var color = diag.Severity switch
            {
                SourceDiagnosticSeverity.Suggestion => "green",
                SourceDiagnosticSeverity.Warning => "yellow",
                SourceDiagnosticSeverity.Error => "red",
                _ => throw new UnreachableException(),
            };

            AnsiConsole.MarkupLine(culture, $"[{color}]{{0}}[/]", diag.ToString().EscapeMarkup());

            PrintContext(leading);

            foreach (var target in targets)
            {
                PrintLine(target.Line, target.Text, "bold");

                var sb = new StringBuilder();

                var isStart = target.Line == start.Line;
                var isEnd = target.Line == end.Line;

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

                AnsiConsole.MarkupLine(culture, $"[{color}]    {{0, {margin}}}[/]", sb.ToString().EscapeMarkup());
            }

            PrintContext(trailing);

            AnsiConsole.WriteLine();
        }

        return semantics.HasErrors ? 1 : 0;
    }
}
