namespace Vezel.Celerity.Tests.Adapter;

internal sealed class CelerityTestResult
{
    public required TestOutcome Outcome { get; init; }

    public required string? Error { get; init; }

    public required string StandardOut { get; init; }

    public required string StandardError { get; init; }
}
