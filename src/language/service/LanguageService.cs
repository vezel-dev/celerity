using Vezel.Celerity.Language.Service.Logging;

namespace Vezel.Celerity.Language.Service;

public sealed class LanguageService : IDisposable
{
    public Task Completion { get; }

    private readonly TaskCompletionSource _disposed = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly LanguageServer _server;

    private LanguageService(LanguageServer server)
    {
        _server = server;
        Completion = Task.WhenAny(server.WaitForExit, _disposed.Task);
    }

    public static ValueTask<LanguageService> CreateAsync(
        LanguageServiceConfiguration configuration, CancellationToken cancellationToken = default)
    {
        Check.Null(configuration);

        return CreateAsync();

        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
        [SuppressMessage("", "CA2000")]
        async ValueTask<LanguageService> CreateAsync()
        {
            T GetAttribute<T>()
                where T : Attribute
            {
#pragma warning disable CS0436 // TODO: https://github.com/dotnet/Nerdbank.GitVersioning/issues/555
                return typeof(ThisAssembly).Assembly.GetCustomAttribute<T>()!;
#pragma warning restore CS0436
            }

            return new(
                await LanguageServer.From(
                    new LanguageServerOptions()
                        .WithServerInfo(new()
                        {
                            Name = GetAttribute<AssemblyProductAttribute>()!.Product,
                            Version = GetAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion,
                        })
                        .WithInput(configuration.Input)
                        .WithOutput(configuration.Output)
                        .WithContentModifiedSupport(true)
                        .WithMaximumRequestTimeout(configuration.RequestTimeout)
                        .ConfigureLogging(builder =>
                        {
                            _ = builder.SetMinimumLevel(configuration.LogLevel switch
                            {
                                Logging.LogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
                                Logging.LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
                                Logging.LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
                                Logging.LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
                                Logging.LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
                                Logging.LogLevel.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
                                _ => throw new UnreachableException(),
                            });

                            if (configuration.LoggerProvider is { } provider)
                                _ = builder.AddProvider(new LanguageServiceLoggerProviderAdapter(provider));

                            if (configuration.ProtocolLogging)
                                _ = builder.AddLanguageProtocolLogging();
                        }),
                    cancellationToken).ConfigureAwait(false));
        }
    }

    public void Dispose()
    {
        _server.Dispose();

        _ = _disposed.TrySetResult();
    }
}
