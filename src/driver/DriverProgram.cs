using Vezel.Celerity.Driver.Verbs;

namespace Vezel.Celerity.Driver;

[SuppressMessage("", "CA1812")] // TODO: https://github.com/dotnet/roslyn-analyzers/issues/6218
internal sealed class DriverProgram : IProgram
{
    public static async Task RunAsync(ProgramContext context)
    {
        using var parser = new Parser(static settings =>
        {
            settings.GetoptMode = true;
            settings.PosixlyCorrect = true;
            settings.CaseSensitive = false;
            settings.CaseInsensitiveEnumValues = true;
            settings.HelpWriter = Terminal.StandardError.TextWriter;
        });

        context.ExitCode = await parser
            .ParseArguments(
                context.Arguments.ToArray(),
#pragma warning disable CS0436 // TODO: https://github.com/dotnet/Nerdbank.GitVersioning/issues/555
                typeof(ThisAssembly)
#pragma warning restore CS0436
                    .Assembly
                    .DefinedTypes
                    .Where(static type => type.GetCustomAttribute<VerbAttribute>() != null)
                    .ToArray())
            .MapResult(
                verb => Unsafe.As<Verb>(verb).RunWithHandlerAsync(context.CancellationToken),
                static _ => ValueTask.FromResult(1));
    }
}
