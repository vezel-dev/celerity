using Vezel.Celerity.Driver.Verbs;

using var parser = new Parser(settings =>
{
    settings.GetoptMode = true;
    settings.PosixlyCorrect = true;
    settings.CaseSensitive = false;
    settings.CaseInsensitiveEnumValues = true;
    settings.HelpWriter = Console.Error;
});

return await parser
    .ParseArguments(
        args,
#pragma warning disable CS0436 // TODO: https://github.com/dotnet/Nerdbank.GitVersioning/issues/555
        typeof(ThisAssembly)
#pragma warning restore CS0436
            .Assembly
            .DefinedTypes
            .Where(t => t.GetCustomAttribute<VerbAttribute>() != null)
            .ToArray())
    .MapResult(
        verb => Unsafe.As<Verb>(verb).RunAsync(),
        _ => Task.FromResult(1));
