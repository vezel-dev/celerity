namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("info", HelpText = "Print Celerity runtime environment information.")]
internal sealed class InfoVerb : Verb
{
    public override Task<int> RunAsync()
    {
        var culture = CultureInfo.InvariantCulture;

        AnsiConsole.MarkupLine("[greenyellow]Celerity[/]");

#pragma warning disable CS0436 // TODO: https://github.com/dotnet/Nerdbank.GitVersioning/issues/555
        AnsiConsole.MarkupLineInterpolated(
            culture, $"[lightsteelblue]Version:[/] [white]{ThisAssembly.AssemblyInformationalVersion}[/]");
        AnsiConsole.MarkupLineInterpolated(
            culture, $"[lightsteelblue]Commit:[/] [white]{ThisAssembly.GitCommitId}[/]");
        AnsiConsole.MarkupLineInterpolated(
            culture, $"[lightsteelblue]Date:[/] [white]{ThisAssembly.GitCommitDate}[/]");
        AnsiConsole.MarkupLineInterpolated(
            culture, $"[lightsteelblue]Configuration:[/] [white]{ThisAssembly.AssemblyConfiguration}[/]");
#pragma warning restore CS0436

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[greenyellow].NET[/]");

        AnsiConsole.MarkupLineInterpolated(
            culture, $"[lightsteelblue]Version:[/] [white]{Environment.Version}[/]");
        AnsiConsole.MarkupLineInterpolated(
            culture, $"[lightsteelblue]Architecture:[/] [white]{RuntimeInformation.ProcessArchitecture}[/]");
        AnsiConsole.MarkupLineInterpolated(
            culture, $"[lightsteelblue]RID:[/] [white]{RuntimeInformation.RuntimeIdentifier}[/]");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[greenyellow]System[/]");

        AnsiConsole.MarkupLineInterpolated(
            culture, $"[lightsteelblue]Version:[/] [white]{RuntimeInformation.OSDescription}[/]");
        AnsiConsole.MarkupLineInterpolated(
            culture, $"[lightsteelblue]Architecture:[/] [white]{RuntimeInformation.OSArchitecture}[/]");
        AnsiConsole.MarkupLineInterpolated(
            culture, $"[lightsteelblue]Processors:[/] [white]{Environment.ProcessorCount}[/]");

        return Task.FromResult(0);
    }
}
