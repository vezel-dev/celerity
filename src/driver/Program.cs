using Vezel.Celerity.Driver.Verbs;

namespace Vezel.Celerity.Driver;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        using var parser = new Parser(static settings =>
        {
            settings.GetoptMode = true;
            settings.PosixlyCorrect = true;
            settings.CaseSensitive = false;
            settings.CaseInsensitiveEnumValues = true;
            settings.HelpWriter = Terminal.StandardError.TextWriter;
        });

        using var cts = new CancellationTokenSource();

        Terminal.Signaled += ctx =>
        {
            ctx.Cancel = true;

            cts.Cancel();
        };

        return await parser
            .ParseArguments(
                args,
                [.. typeof(ThisAssembly)
                    .Assembly
                    .DefinedTypes
                    .Where(static type => type.GetCustomAttribute<VerbAttribute>() != null)])
            .MapResult(
                verb => Unsafe.As<Verb>(verb).RunWithHandlerAsync(cts.Token),
                static _ => ValueTask.FromResult(1));
    }
}
