namespace Vezel.Celerity.Language.Service.Logging;

internal sealed class LanguageServiceLoggerProviderAdapter : ILoggerProvider
{
    private readonly LanguageServiceLoggerProvider _provider;

    public LanguageServiceLoggerProviderAdapter(LanguageServiceLoggerProvider provider)
    {
        _provider = provider;
    }

    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        Check.Null(categoryName);

        var logger = _provider.CreateLogger(categoryName);

        Check.Operation(logger != null);

        return new LanguageServiceLoggerAdapter(logger);
    }
}
