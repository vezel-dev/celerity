using Vezel.Celerity.Driver.IO;

namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("info", HelpText = "Print Celerity runtime environment information.")]
internal sealed class InfoVerb : Verb
{
    private static readonly IEnumerable<(string Name, object Value)> _celerity =
        new (string, object)[]
        {
#pragma warning disable CS0436 // TODO: https://github.com/dotnet/Nerdbank.GitVersioning/issues/555
            ("Version", ThisAssembly.AssemblyInformationalVersion),
            ("Commit", ThisAssembly.GitCommitId),
            ("Date", ThisAssembly.GitCommitDate.ToString("o", CultureInfo.InvariantCulture)),
            ("Mode", ThisAssembly.AssemblyConfiguration),
#pragma warning restore CS0436
        };

    private static readonly IEnumerable<(string Name, object Value)> _runtime =
        new (string, object)[]
        {
            ("Version", Environment.Version),
            ("RID", RuntimeInformation.RuntimeIdentifier),
        };

    private static readonly IEnumerable<(string Name, object Value)> _process =
        new (string, object)[]
        {
            ("Architecture", RuntimeInformation.ProcessArchitecture),
            ("CPUs", Environment.ProcessorCount),
            ("Privileged", Environment.IsPrivilegedProcess),
        };

    private static readonly IEnumerable<(string Name, object Value)> _system =
        new (string, object)[]
        {
            ("Version", RuntimeInformation.OSDescription),
            ("Architecture", RuntimeInformation.OSArchitecture),
        };

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    protected override async ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
        async ValueTask WriteSectionAsync(string header, IEnumerable<(string Name, object Value)> table)
        {
            await Out.WriteControlAsync(ControlSequences.SetForegroundColor(175, 255, 0), cancellationToken);
            await Out.WriteLineAsync(header, cancellationToken);
            await Out.WriteControlAsync(ControlSequences.ResetAttributes(), cancellationToken);

            foreach (var (name, value) in table)
            {
                await Out.WriteControlAsync(ControlSequences.SetForegroundColor(215, 215, 255), cancellationToken);
                await Out.WriteAsync($"{name}: ", cancellationToken);
                await Out.WriteControlAsync(ControlSequences.ResetAttributes(), cancellationToken);
                await Out.WriteLineAsync(value, cancellationToken);
            }
        }

        await WriteSectionAsync("Celerity", _celerity);
        await Out.WriteLineAsync(cancellationToken);
        await WriteSectionAsync(".NET", _runtime);
        await Out.WriteLineAsync(cancellationToken);
        await WriteSectionAsync("Process", _process);
        await Out.WriteLineAsync(cancellationToken);
        await WriteSectionAsync("System", _system);

        return 0;
    }
}
