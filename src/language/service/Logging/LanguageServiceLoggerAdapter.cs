namespace Vezel.Celerity.Language.Service.Logging;

internal sealed class LanguageServiceLoggerAdapter : ILogger
{
    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        private NullScope()
        {
        }

        public void Dispose()
        {
        }
    }

    private readonly LanguageServiceLogger _logger;

    public LanguageServiceLoggerAdapter(LanguageServiceLogger logger)
    {
        _logger = logger;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        // TODO: Should we support scopes?
        return NullScope.Instance;
    }

    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
    {
        return logLevel != Microsoft.Extensions.Logging.LogLevel.None;
    }

    public void Log<TState>(
        Microsoft.Extensions.Logging.LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var msg = formatter(state, exception);

        if (!string.IsNullOrWhiteSpace(msg) || exception != null)
            _logger.Log(
                logLevel switch
                {
                    Microsoft.Extensions.Logging.LogLevel.Trace => LogLevel.Trace,
                    Microsoft.Extensions.Logging.LogLevel.Debug => LogLevel.Debug,
                    Microsoft.Extensions.Logging.LogLevel.Information => LogLevel.Information,
                    Microsoft.Extensions.Logging.LogLevel.Warning => LogLevel.Warning,
                    Microsoft.Extensions.Logging.LogLevel.Error => LogLevel.Error,
                    Microsoft.Extensions.Logging.LogLevel.Critical => LogLevel.Critical,
                    _ => throw new UnreachableException(),
                },
                eventId.ToString(),
                msg,
                exception);
    }
}
