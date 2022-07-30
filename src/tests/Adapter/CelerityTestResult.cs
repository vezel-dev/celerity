namespace Vezel.Celerity.Tests.Adapter;

internal sealed class CelerityTestResult
{
    public TestOutcome Outcome { get; }

    public string? Error { get; }

    public string StandardOut { get; }

    public string StandardError { get; }

    public CelerityTestResult(TestOutcome outcome, string? error, string stdout, string stderr)
    {
        Outcome = outcome;
        Error = error;
        StandardOut = stdout;
        StandardError = stderr;
    }
}
