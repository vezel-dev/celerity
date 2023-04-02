using Vezel.Celerity.Driver.IO;

namespace Vezel.Celerity.Driver.Verbs;

internal abstract class Verb
{
    protected static TerminalReader In { get; } = Terminal.StandardIn;

    protected static TerminalWriter Out { get; } = Terminal.StandardOut;

    protected static TerminalWriter Error { get; } = Terminal.StandardError;

    private static readonly Color _errorColor = Color.FromArgb(225, 0, 0);

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
                ControlSequences.SetForegroundColor(_errorColor.R, _errorColor.G, _errorColor.B), cancellationToken);
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
            try
            {
                workspace = await ProjectWorkspace.OpenAsync(directory, disableAnalysis, cancellationToken);
            }
            catch (PathTooLongException)
            {
                throw new DriverException($"Workspace path '{directory}' is too long.");
            }
            catch (DirectoryNotFoundException)
            {
                throw new DriverException($"Could not find part of workspace path '{directory}'.");
            }
            catch (IOException ex) when (ex is not FileNotFoundException)
            {
                throw new DriverException(
                    $"I/O error while reading '{ProjectWorkspace.ConfigurationFileName}': {ex.Message}");
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException)
            {
                throw new DriverException("Access to the workspace directory was denied.");
            }
            catch (JsonException ex)
            {
                throw new DriverException(
                    $"'{ProjectWorkspace.ConfigurationFileName}' contains invalid JSON: {ex.Message}");
            }
            catch (ProjectException ex)
            {
                throw new DriverException($"'{ProjectWorkspace.ConfigurationFileName}': {ex.Message}");
            }
        }
        catch (FileNotFoundException)
        {
            // No configuration file, so try a simple workspace.
            workspace = SimpleWorkspace.Open(directory, disableAnalysis);
        }

        PhysicalWorkspaceWatcher.Populate(workspace);

        return workspace;
    }
}
