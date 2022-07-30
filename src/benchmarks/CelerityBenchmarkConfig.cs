namespace Vezel.Celerity.Benchmarks;

internal sealed class CelerityBenchmarkConfig : ManualConfig
{
    public CelerityBenchmarkConfig(bool test, string? filter)
    {
        var job = Job.InProcess;

        if (test)
            job = job.WithStrategy(RunStrategy.ColdStart).WithIterationCount(1);
        else
            _ = AddValidator(JitOptimizationsValidator.FailOnError);

        if (filter != null)
            _ = AddFilter(new GlobFilter(new[] { filter }));

        _ = WithOptions(ConfigOptions.JoinSummary | ConfigOptions.StopOnFirstError)
            .WithArtifactsPath(Path.Combine(Path.GetDirectoryName(Environment.ProcessPath!)!, "artifacts"))
            .WithSummaryStyle(new(CultureInfo.InvariantCulture, false, null, TimeUnit.Microsecond))
            .AddAnalyser(
                BaselineCustomAnalyzer.Default,
                EnvironmentAnalyser.Default,
                MinIterationTimeAnalyser.Default,
                OutliersAnalyser.Default,
                RuntimeErrorAnalyser.Default,
                ZeroMeasurementAnalyser.Default)
            .AddValidator(
                BaselineValidator.FailOnError,
                CompilationValidator.FailOnError,
                ConfigCompatibilityValidator.FailOnError,
                DeferredExecutionValidator.FailOnError,
                DiagnosersValidator.Composite,
                ExecutionValidator.FailOnError,
                GenericBenchmarksValidator.DontFailOnError,
                ParamsAllValuesValidator.FailOnError,
                ReturnValueValidator.FailOnError,
                RunModeValidator.FailOnError,
                SetupCleanupValidator.FailOnError,
                ShadowCopyValidator.DontFailOnError)
            .AddColumn(
                TargetMethodColumn.Method,
                StatisticColumn.Mean,
                StatisticColumn.StdErr,
                StatisticColumn.StdDev,
                StatisticColumn.Min,
                StatisticColumn.Median,
                StatisticColumn.Max,
                StatisticColumn.Iterations,
                StatisticColumn.OperationsPerSecond)
            .AddExporter(MarkdownExporter.GitHub)
            .AddLogger(CelerityBenchmarkLogger.Instance)
            .AddJob(job);
    }
}
