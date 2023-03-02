namespace Vezel.Celerity.Benchmarks;

internal sealed class CelerityBenchmarkLogger : ILogger
{
    public static CelerityBenchmarkLogger Instance { get; } = new();

    public string Id => nameof(CelerityBenchmarkLogger);

    public int Priority => 0;

    private static readonly bool _interactive = Terminal.TerminalOut.IsInteractive;

    private static readonly ImmutableDictionary<LogKind, (byte R, byte G, byte B)> _colors =
        new Dictionary<LogKind, (byte, byte, byte)>
        {
            [LogKind.Default] = (135, 135, 175),
            [LogKind.Help] = (175, 95, 0),
            [LogKind.Header] = (175, 255, 0),
            [LogKind.Result] = (135, 135, 255),
            [LogKind.Statistic] = (175, 175, 255),
            [LogKind.Info] = (255, 255, 255),
            [LogKind.Error] = (255, 0, 0),
            [LogKind.Hint] = (0, 175, 135),
        }.ToImmutableDictionary();

    private CelerityBenchmarkLogger()
    {
    }

    private static void WriteControl(string sequence)
    {
        if (_interactive)
            Terminal.Out(sequence);
    }

    private void Write(LogKind kind, string text, bool eol)
    {
        var (r, g, b) = _colors[kind];

        WriteControl(ControlSequences.SetForegroundColor(r, g, b));
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
