namespace Vezel.Celerity.Driver.Commands;

[SuppressMessage("", "CA1812")]
internal sealed class InfoCommand : AsyncCommand<InfoCommand.InfoCommandSettings>
{
    public sealed class InfoCommandSettings : CommandSettings
    {
    }

    public override Task<int> ExecuteAsync(CommandContext context, InfoCommandSettings settings)
    {
        var culture = CultureInfo.InvariantCulture;

        AnsiConsole.MarkupLine("[greenyellow]Celerity[/]");

        // TODO: https://github.com/dotnet/Nerdbank.GitVersioning/issues/555
#pragma warning disable CS0436
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
