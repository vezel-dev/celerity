namespace Vezel.Celerity.Driver.Logging;

internal sealed class TerminalLanguageServiceLoggerProvider : LanguageServiceLoggerProvider
{
    public TerminalWriter Writer { get; }

    public TerminalLanguageServiceLoggerProvider(TerminalWriter writer)
    {
        Writer = writer;
    }

    public override TerminalLanguageServiceLogger CreateLogger(string name)
    {
        return new(this, name);
    }
}
