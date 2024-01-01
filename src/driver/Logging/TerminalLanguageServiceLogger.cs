using Vezel.Celerity.Driver.IO;

namespace Vezel.Celerity.Driver.Logging;

internal sealed class TerminalLanguageServiceLogger : LanguageServiceLogger
{
    private static readonly Color _timestampColor = Color.FromArgb(127, 127, 127);

    private static readonly Color _nameColor = Color.FromArgb(233, 233, 233);

    private static readonly Color _eventColor = Color.FromArgb(0, 155, 155);

    private static readonly Color _traceColor = Color.FromArgb(127, 0, 127);

    private static readonly Color _debugColor = Color.FromArgb(0, 127, 255);

    private static readonly Color _informationColor = Color.FromArgb(255, 255, 255);

    private static readonly Color _warningColor = Color.FromArgb(255, 255, 0);

    private static readonly Color _errorColor = Color.FromArgb(255, 63, 0);

    private static readonly Color _criticalColor = Color.FromArgb(255, 0, 0);

    private readonly TerminalLanguageServiceLoggerProvider _provider;

    private readonly string _name;

    public TerminalLanguageServiceLogger(TerminalLanguageServiceLoggerProvider provider, string name)
    {
        _provider = provider;
        _name = name;
    }

    public override void Log(LogLevel logLevel, string eventName, string message, Exception? exception)
    {
        var writer = _provider.Writer;

        writer.Write("[");
        writer.WriteControl(ControlSequences.SetForegroundColor(_timestampColor));
        writer.Write($"{DateTime.Now:HH:mm:ss.fff}");
        writer.WriteControl(ControlSequences.ResetAttributes());
        writer.Write("]");

        var (level, color) = logLevel switch
        {
            LogLevel.Trace => ("TRC", _traceColor),
            LogLevel.Debug => ("DBG", _debugColor),
            LogLevel.Information => ("INF", _informationColor),
            LogLevel.Warning => ("WRN", _warningColor),
            LogLevel.Error => ("ERR", _errorColor),
            LogLevel.Critical => ("CRT", _criticalColor),
            _ => throw new UnreachableException(),
        };

        writer.Write("[");
        writer.WriteControl(ControlSequences.SetForegroundColor(color));
        writer.Write(level);
        writer.WriteControl(ControlSequences.ResetAttributes());
        writer.Write("]");

        writer.Write("[");
        writer.WriteControl(ControlSequences.SetForegroundColor(_nameColor));
        writer.Write(_name);
        writer.WriteControl(ControlSequences.ResetAttributes());
        writer.Write("]");

        writer.Write("[");
        writer.WriteControl(ControlSequences.SetForegroundColor(_eventColor));
        writer.Write(eventName);
        writer.WriteControl(ControlSequences.ResetAttributes());
        writer.Write("] ");

        var hasMessage = string.IsNullOrWhiteSpace(message);

        if (hasMessage)
            writer.Write(message);

        if (exception != null)
        {
            if (hasMessage)
                writer.WriteLine();

            writer.Write(exception);
        }
    }
}
