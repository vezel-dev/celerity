namespace Vezel.Celerity.Benchmarks;

[SuppressMessage("", "CA1812")] // TODO: https://github.com/dotnet/roslyn-analyzers/issues/6218
internal sealed class BenchmarkProgram : IProgram
{
    public static Task RunAsync(ProgramContext context)
    {
        using var parser = new Parser(settings =>
        {
            settings.GetoptMode = true;
            settings.PosixlyCorrect = true;
            settings.CaseSensitive = false;
            settings.CaseInsensitiveEnumValues = true;
            settings.HelpWriter = Terminal.StandardError.TextWriter;
        });

        return Task.FromResult(
            parser
                .ParseArguments<BenchmarkOptions>(context.Arguments.ToArray())
                .MapResult(
                    opts =>
                        BenchmarkSwitcher
                            .FromAssembly(typeof(ThisAssembly).Assembly)
                            .RunAll(new CelerityBenchmarkConfig(opts.Test, opts.Filter))
                            .Any(s => s.HasCriticalValidationErrors)
                            ? 1
                            : 0,
                    _ => 1));
    }
}
