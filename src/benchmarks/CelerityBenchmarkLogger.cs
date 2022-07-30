namespace Vezel.Celerity.Benchmarks;

internal sealed class CelerityBenchmarkLogger : ILogger
{
    public static CelerityBenchmarkLogger Instance { get; } = new();

    public string Id => nameof(CelerityBenchmarkLogger);

    public int Priority => 0;

    private static readonly IReadOnlyDictionary<LogKind, Color> _colors = new Dictionary<LogKind, Color>
    {
        [LogKind.Default] = Color.LightSlateGrey,
        [LogKind.Help] = Color.DarkOrange,
        [LogKind.Header] = Color.GreenYellow,
        [LogKind.Result] = Color.LightSlateBlue,
        [LogKind.Statistic] = Color.LightSteelBlue,
        [LogKind.Info] = Color.White,
        [LogKind.Error] = Color.Red,
        [LogKind.Hint] = Color.DarkCyan,
    };

    private CelerityBenchmarkLogger()
    {
    }

    private void Write(LogKind kind, string text, bool eol)
    {
        AnsiConsole.Markup(CultureInfo.InvariantCulture, "[{0}]{1}[/]", _colors[kind], text.EscapeMarkup());

        if (eol)
            WriteLine();
    }

    public void Write(LogKind logKind, string text)
    {
        Write(logKind, text, false);
    }

    public void WriteLine(LogKind logKind, string text)
    {
        Write(logKind, text, true);
    }

    public void WriteLine()
    {
        AnsiConsole.WriteLine();
    }

    public void Flush()
    {
    }
}
