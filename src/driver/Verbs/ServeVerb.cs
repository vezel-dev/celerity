using Vezel.Celerity.Driver.Logging;

namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("serve", HelpText = "Host the Celerity language server.")]
internal sealed class ServeVerb : Verb
{
    [Value(0, HelpText = "Project directory.")]
    public required string? Directory { get; init; }

    [Option('l', "level", Default = LogLevel.Information, HelpText = "Set log level.")]
    public required LogLevel Level { get; init; }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    protected override async ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        if (Directory != null && string.IsNullOrWhiteSpace(Directory))
            throw new DriverException($"Invalid workspace path '{Directory}'.");

        await Error.WriteLineAsync("Running Celerity language server on standard input/output.", cancellationToken);

        using var service = await LanguageService.CreateAsync(
            new LanguageServiceConfiguration(In.Stream, Out.Stream)
                .WithLogLevel(Level)
                .WithLoggerProvider(new TerminalLanguageServiceLoggerProvider(Error))
                .WithProtocolLogging(protocolLogging: true),
            cancellationToken);

        await service.Completion;

        return 0;
    }
}
