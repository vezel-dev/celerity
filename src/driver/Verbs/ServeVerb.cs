using Vezel.Celerity.Driver.Logging;

namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("serve", HelpText = "Host the Celerity language server.")]
internal sealed class ServeVerb : Verb
{
    [Option('w', "workspace", HelpText = "Set workspace directory.")]
    public required string? Workspace { get; init; }

    [Option('l', "level", Default = LogLevel.Information, HelpText = "Set log level.")]
    public required LogLevel Level { get; init; }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    protected override async ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        if (Workspace != null && string.IsNullOrWhiteSpace(Workspace))
            throw new DriverException($"Invalid workspace path '{Workspace}'.");

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
