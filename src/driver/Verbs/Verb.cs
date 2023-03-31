using Vezel.Celerity.Driver.IO;

namespace Vezel.Celerity.Driver.Verbs;

internal abstract class Verb
{
    protected static TerminalReader In { get; } = Terminal.StandardIn;

    protected static TerminalWriter Out { get; } = Terminal.StandardOut;

    protected static TerminalWriter Error { get; } = Terminal.StandardError;

    private static readonly Color _color = Color.FromArgb(225, 0, 0);

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public async ValueTask<int> RunWithHandlerAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await RunAsync(cancellationToken);
        }
        catch (DriverException ex)
        {
            await Error.WriteControlAsync(
                ControlSequences.SetForegroundColor(_color.R, _color.G, _color.B), cancellationToken);
            await Error.WriteLineAsync(ex.Message, cancellationToken);
            await Error.WriteControlAsync(ControlSequences.ResetAttributes(), cancellationToken);

            return 1;
        }
    }

    protected abstract ValueTask<int> RunAsync(CancellationToken cancellationToken);

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    protected static async ValueTask<Workspace> OpenWorkspaceAsync(
        string? directory, bool disableAnalysis, CancellationToken cancellationToken)
    {
        directory ??= Environment.CurrentDirectory;

        Workspace workspace;

        try
        {
            workspace = await ProjectWorkspace.OpenAsync(directory, disableAnalysis, cancellationToken);
        }
        catch (FileNotFoundException)
        {
            workspace = SimpleWorkspace.Open(directory, disableAnalysis);
        }

        PhysicalWorkspaceWatcher.Populate(workspace);

        return workspace;
    }
}
