// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Language.Service.Logging;

namespace Vezel.Celerity.Language.Service;

public sealed class LanguageServiceConfiguration
{
    public Stream Input { get; private set; }

    public Stream Output { get; private set; }

    public TimeSpan RequestTimeout { get; private set; } = Timeout.InfiniteTimeSpan;

    public Logging.LogLevel LogLevel { get; private set; } = Logging.LogLevel.Information;

    public LanguageServiceLoggerProvider? LoggerProvider { get; private set; }

    public bool ProtocolLogging { get; private set; }

    private LanguageServiceConfiguration()
    {
        Input = null!;
        Output = null!;
    }

    public LanguageServiceConfiguration(Stream input, Stream output)
    {
        Check.Null(input);
        Check.Argument(input.CanRead, input);
        Check.Null(output);
        Check.Argument(output.CanWrite, output);

        Input = input;
        Output = output;
    }

    private LanguageServiceConfiguration Clone()
    {
        return new()
        {
            Input = Input,
            Output = Output,
            RequestTimeout = RequestTimeout,
            LogLevel = LogLevel,
            LoggerProvider = LoggerProvider,
            ProtocolLogging = ProtocolLogging,
        };
    }

    public LanguageServiceConfiguration WithInput(Stream input)
    {
        Check.Null(input);
        Check.Argument(input.CanRead, input);

        var cfg = Clone();

        cfg.Input = input;

        return cfg;
    }

    public LanguageServiceConfiguration WithOutput(Stream output)
    {
        Check.Null(output);
        Check.Argument(output.CanWrite, output);

        var cfg = Clone();

        cfg.Output = output;

        return cfg;
    }

    public LanguageServiceConfiguration WithRequestTimeout(TimeSpan requestTimeout)
    {
        Check.Range((long)requestTimeout.TotalMilliseconds is >= -1 and <= int.MaxValue, requestTimeout);

        var cfg = Clone();

        cfg.RequestTimeout = requestTimeout;

        return cfg;
    }

    public LanguageServiceConfiguration WithLogLevel(Logging.LogLevel logLevel)
    {
        Check.Enum(logLevel);

        var cfg = Clone();

        cfg.LogLevel = logLevel;

        return cfg;
    }

    public LanguageServiceConfiguration WithLoggerProvider(LanguageServiceLoggerProvider loggerProvider)
    {
        Check.Null(loggerProvider);

        var cfg = Clone();

        cfg.LoggerProvider = loggerProvider;

        return cfg;
    }

    public LanguageServiceConfiguration WithProtocolLogging(bool protocolLogging)
    {
        var cfg = Clone();

        cfg.ProtocolLogging = protocolLogging;

        return cfg;
    }
}
