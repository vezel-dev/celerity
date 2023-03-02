using Vezel.Celerity.Benchmarks;

using var parser = new Parser(settings =>
{
    settings.GetoptMode = true;
    settings.PosixlyCorrect = true;
    settings.CaseSensitive = false;
    settings.CaseInsensitiveEnumValues = true;
    settings.HelpWriter = Console.Error;
});

return parser
    .ParseArguments<BenchmarkOptions>(args)
    .MapResult(
        opts =>
            BenchmarkSwitcher
                .FromAssembly(typeof(ThisAssembly).Assembly)
                .RunAll(new CelerityBenchmarkConfig(opts.Test, opts.Filter))
                .Any(s => s.HasCriticalValidationErrors)
                ? 1
                : 0,
        _ => 1);
