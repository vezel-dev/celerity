namespace Vezel.Celerity.Benchmarks;

internal sealed class CelerityBenchmarkLogger : ILogger
{
    public static CelerityBenchmarkLogger Instance { get; } = new();

    public string Id => nameof(CelerityBenchmarkLogger);

    public int Priority => 0;

    private static readonly bool _interactive = Terminal.StandardOut.IsInteractive;

    private static readonly ImmutableDictionary<LogKind, Color> _colors =
        new Dictionary<LogKind, Color>
        {
            [LogKind.Default] = Color.FromArgb(135, 135, 175),
            [LogKind.Help] = Color.FromArgb(175, 95, 0),
            [LogKind.Header] = Color.FromArgb(175, 255, 0),
            [LogKind.Result] = Color.FromArgb(135, 135, 255),
            [LogKind.Statistic] = Color.FromArgb(175, 175, 255),
            [LogKind.Info] = Color.FromArgb(255, 255, 255),
            [LogKind.Error] = Color.FromArgb(255, 0, 0),
            [LogKind.Hint] = Color.FromArgb(0, 175, 135),
        }.ToImmutableDictionary();

    private CelerityBenchmarkLogger()
    {
    }

    private void Write(LogKind kind, string text, bool eol)
    {
        static void WriteControl(string sequence)
        {
            if (_interactive)
                Terminal.Out(sequence);
        }

        WriteControl(ControlSequences.SetForegroundColor(_colors[kind]));
        Terminal.Out(text);
        WriteControl(ControlSequences.ResetAttributes());

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
        Terminal.OutLine();
    }

    public void Flush()
    {
    }
}
