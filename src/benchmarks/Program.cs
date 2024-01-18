namespace Vezel.Celerity.Benchmarks;

internal static class Program
{
    public static Task<int> Main(string[] args)
    {
        using var parser = new Parser(static settings =>
        {
            settings.GetoptMode = true;
            settings.PosixlyCorrect = true;
            settings.CaseSensitive = false;
            settings.CaseInsensitiveEnumValues = true;
            settings.HelpWriter = Terminal.StandardError.TextWriter;
        });

        return Task.FromResult(
            parser
                .ParseArguments<BenchmarkOptions>(args)
                .MapResult(
                    static opts =>
                        BenchmarkSwitcher
                            .FromAssembly(typeof(ThisAssembly).Assembly)
                            .RunAll(new CelerityBenchmarkConfig(opts.Test, opts.Filter))
                            .Any(s => s.HasCriticalValidationErrors)
                            ? 1
                            : 0,
                    static _ => 1));
    }
}
