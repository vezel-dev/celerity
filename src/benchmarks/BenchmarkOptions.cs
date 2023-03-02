namespace Vezel.Celerity.Benchmarks;

[SuppressMessage("", "CA1812")]
internal sealed class BenchmarkOptions
{
    [Option('f', "filter", HelpText = "Benchmark glob filter.")]
    public required string? Filter { get; init; }

    [Option('t', "test", HelpText = "Enable test mode.")]
    public required bool Test { get; init; }
}
