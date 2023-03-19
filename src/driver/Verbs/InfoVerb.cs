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
            ("Date", ThisAssembly.GitCommitDate),
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
    public override async ValueTask<int> RunAsync()
    {
        var interactive = Terminal.TerminalOut.IsInteractive;

        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
        async ValueTask WriteSectionAsync(string header, IEnumerable<(string Name, object Value)> table)
        {
            [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
            async ValueTask WriteControlAsync(string sequence)
            {
                if (interactive)
                    await Terminal.OutAsync(sequence);
            }

            await WriteControlAsync(ControlSequences.SetForegroundColor(175, 255, 0));
            await Terminal.OutLineAsync(header);
            await WriteControlAsync(ControlSequences.ResetAttributes());

            foreach (var (name, value) in table)
            {
                await WriteControlAsync(ControlSequences.SetForegroundColor(215, 215, 255));
                await Terminal.OutAsync($"{name}: ");
                await WriteControlAsync(ControlSequences.ResetAttributes());
                await Terminal.OutLineAsync(value);
            }
        }

        await WriteSectionAsync("Celerity", _celerity);
        await Terminal.OutLineAsync();
        await WriteSectionAsync(".NET", _runtime);
        await Terminal.OutLineAsync();
        await WriteSectionAsync("Process", _process);
        await Terminal.OutLineAsync();
        await WriteSectionAsync("System", _system);

        return 0;
    }
}
