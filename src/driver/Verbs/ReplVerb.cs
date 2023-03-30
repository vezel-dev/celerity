namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("repl", HelpText = "Start an interactive Celerity session.")]
internal sealed class ReplVerb : Verb
{
    [Value(0, HelpText = "Source code directory.")]
    public required string? Directory { get; init; }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public override async ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        if (!Terminal.StandardIn.IsInteractive)
        {
            await Terminal.ErrorLineAsync("The REPL can only be run in an interactive terminal.", cancellationToken);

            return 1;
        }

        // TODO: Implement this.
        return 0;
    }
}
