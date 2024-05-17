// SPDX-License-Identifier: 0BSD

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

    // We only support communication over standard I/O. This option exists because the LSP specification strongly
    // recommends supporting it, and in practice, LSP clients tend to assume that it is supported.
    [Option('s', "stdio", HelpText = "Enable standard I/O communication.")]
    public required bool Stdio { get; init; }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    protected override async ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        if (Workspace != null && string.IsNullOrWhiteSpace(Workspace))
            throw new DriverException($"Invalid workspace path '{Workspace}'.");

        if (!Stdio)
            throw new DriverException("The language server can only run with standard I/O communication.");

        await Error.WriteLineAsync("Running Celerity language server on standard I/O.", cancellationToken);

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
