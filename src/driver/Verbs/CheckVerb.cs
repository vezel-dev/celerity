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
        if (Directory != null && string.IsNullOrWhiteSpace(Directory))
            throw new DriverException($"Invalid workspace path '{Directory}'.");

        var workspace = await OpenWorkspaceAsync(Directory, disableAnalysis: false, cancellationToken);
        var writer = new DiagnosticWriter(
            new DiagnosticConfiguration()
                .WithWidthMeasurer(static rune => MonospaceWidth.Measure(rune) ?? 0)
                .WithStyle(new TerminalDiagnosticStyle(Error)));
        var errors = false;

        foreach (var doc in workspace.Documents.Values.OrderBy(static kvp => kvp.Path, StringComparer.Ordinal))
        {
            Diagnostic[] diags;

            try
            {
                diags = (await doc.GetDiagnosticsAsync(cancellationToken)).ToArray();
            }
            catch (PathTooLongException)
            {
                throw new DriverException($"Document path '{doc.Path}' is too long.");
            }
            catch (DirectoryNotFoundException)
            {
                throw new DriverException(
                    $"Could not find part of document path '{Path.GetDirectoryName(doc.Path)}'.");
            }
            catch (IOException ex)
            {
                throw new DriverException($"I/O error while reading document '{doc.Path}': {ex.Message}");
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException)
            {
                throw new DriverException($"Access to the document '{doc.Path}' was denied.");
            }

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
