using Vezel.Celerity.Driver.Diagnostics;

namespace Vezel.Celerity.Driver.Verbs;

[SuppressMessage("", "CA1812")]
[Verb("check", HelpText = "Perform semantic and quality analyses on Celerity code.")]
internal sealed class CheckVerb : Verb
{
    [Value(0, HelpText = "Workspace directory.")]
    public required string? Directory { get; init; }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    protected override async ValueTask<int> RunAsync(CancellationToken cancellationToken)
    {
        var workspace = await OpenWorkspaceAsync(Directory, disableAnalysis: false, cancellationToken);
        var writer = new DiagnosticWriter(new DiagnosticConfiguration().WithStyle(new TerminalDiagnosticStyle(Error)));
        var errors = false;

        foreach (var doc in workspace.Documents.Values.OrderBy(static kvp => kvp.Path, StringComparer.Ordinal))
        {
            var diags = (await doc.GetDiagnosticsAsync(cancellationToken)).ToArray();

            for (var i = 0; i < diags.Length; i++)
            {
                await writer.WriteAsync(diags[i], Error.TextWriter, cancellationToken);

                if (i != diags.Length - 1)
                    await Error.WriteLineAsync(cancellationToken);
            }

            errors |= diags.Any(static diag => diag.IsError);
        }

        return errors ? 1 : 0;
    }
}
