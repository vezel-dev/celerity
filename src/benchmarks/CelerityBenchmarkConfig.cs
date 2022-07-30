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

        _ = WithOptions(ConfigOptions.JoinSummary | ConfigOptions.StopOnFirstError | ConfigOptions.DisableLogFile)
            .WithSummaryStyle(new(CultureInfo.InvariantCulture, false, null, TimeUnit.Microsecond, true))
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
            .AddLogger(ConsoleLogger.Unicode)
            .AddJob(job);
    }
}
