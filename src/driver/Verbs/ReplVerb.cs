namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("repl", HelpText = "Start an interactive Celerity session.")]
internal sealed class ReplVerb : Verb
{
    [Value(0, HelpText = "Source code directory.")]
    public required string? Directory { get; init; }

    public override async ValueTask<int> RunAsync()
    {
        if (!Terminal.TerminalIn.IsInteractive)
        {
            await Terminal.ErrorLineAsync("The REPL can only be run in an interactive terminal.");

            return 1;
        }

        // TODO: Implement this.
        return 0;
    }
}
