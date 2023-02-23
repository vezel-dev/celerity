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
            .EnumerateLines()
            .Select((line, i) => (Line: i + 1, Text: line.Text.ReplaceLineEndings(string.Empty)))
            .ToArray();

        foreach (var diag in semantics.Diagnostics)
        {
            void PrintContext(IReadOnlyList<(int Line, string Text)> lines)
            {
                if (!lines.All(t => string.IsNullOrWhiteSpace(t.Text)))
                    foreach (var (line, text) in lines)
                        PrintLine(line, text, null);
            }

            var margin = lines[^1].Line.ToString(culture).Length;

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
            var leading = lines.Where(t => t.Line >= location.Line - Context && t.Line < location.Line).ToArray();
            var target = lines.SingleOrDefault(t => t.Line == location.Line);
            var trailing = lines.Where(t => t.Line > location.Line && t.Line <= location.Line + Context).ToArray();
            var color = diag.Severity switch
            {
                SourceDiagnosticSeverity.Suggestion => "green",
                SourceDiagnosticSeverity.Warning => "yellow",
                SourceDiagnosticSeverity.Error => "red",
                _ => throw new UnreachableException(),
            };

            AnsiConsole.MarkupLine(culture, $"[{color}]{{0}}[/]", diag.ToString().EscapeMarkup());

            PrintContext(leading);
            PrintLine(target.Line, target.Text, "bold");

            AnsiConsole.MarkupLine(culture, $"[{color}]   {{0, {margin + location.Character}}}[/]", "^");

            PrintContext(trailing);

            AnsiConsole.WriteLine();
        }

        return semantics.HasErrors ? 1 : 0;
    }
}
