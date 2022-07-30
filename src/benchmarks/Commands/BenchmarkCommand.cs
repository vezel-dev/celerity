namespace Vezel.Celerity.Benchmarks.Commands;

[SuppressMessage("", "CA1812")]
internal sealed class BenchmarkCommand : Command<BenchmarkCommand.BenchmarkCommandSettings>
{
    public sealed class BenchmarkCommandSettings : CommandSettings
    {
        [CommandArgument(0, "[filter]")]
        [Description("Benchmark glob filter")]
        public string? Filter { get; init; }

        [CommandOption("--test")]
        [Description("Enable test mode")]
        public bool Test { get; init; }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] BenchmarkCommandSettings settings)
    {
        return BenchmarkSwitcher.FromAssembly(typeof(ThisAssembly).Assembly)
            .RunAll(new CelerityBenchmarkConfig(settings.Test, settings.Filter))
            .Any(s => s.HasCriticalValidationErrors)
            ? 1
            : 0;
    }
}
