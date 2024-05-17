// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Tooling.Workspaces;

public sealed class PhysicalWorkspaceWatcher : WorkspaceWatcher, IDisposable
{
    private const string Filter = "?*.cel";

    // TODO: Review the possible exceptions from FileSystemWatcher.Error and revise this.
    public static Func<Exception, bool> DefaultRetry { get; } = ex => ex is not Win32Exception;

    public Func<Exception, bool> Retry
    {
        get => _retry;
        set
        {
            Check.Null(value);

            _retry = value;
        }
    }

    private volatile Func<Exception, bool> _retry = DefaultRetry;

    private volatile FileSystemWatcher? _fsw;

    private PhysicalWorkspaceWatcher(Workspace workspace, bool watch)
        : base(workspace)
    {
        foreach (var file in Directory.EnumerateFiles(Workspace.Path, Filter, SearchOption.AllDirectories))
            AddDocument(GetPath(file));

        if (watch)
            _fsw = CreateWatcher();
    }

    public static void Populate(Workspace workspace)
    {
        using (new PhysicalWorkspaceWatcher(workspace, watch: false))
        {
            // Just perform the initial scan.
        }
    }

    public static PhysicalWorkspaceWatcher Create(Workspace workspace)
    {
        return new(workspace, watch: true);
    }

    public void Dispose()
    {
        _fsw?.Dispose();
    }

    private FileSystemWatcher CreateWatcher()
    {
        var fsw = new FileSystemWatcher(Workspace.Path, Filter)
        {
            IncludeSubdirectories = true,
            NotifyFilter =
                NotifyFilters.FileName |
                NotifyFilters.DirectoryName |
                NotifyFilters.Attributes |
                NotifyFilters.Size |
                NotifyFilters.LastWrite |
                NotifyFilters.CreationTime |
                NotifyFilters.Security,
            InternalBufferSize = ushort.MaxValue,
        };

        fsw.Error += HandleError;
        fsw.Created += HandleCreated;
        fsw.Changed += HandleChanged;
        fsw.Renamed += HandleRenamed;
        fsw.Deleted += HandleDeleted;

        fsw.EnableRaisingEvents = true;

        return fsw;
    }

    private string GetPath(string path)
    {
        return Path.GetRelativePath(Workspace.Path, path);
    }

    private void HandleCreated(object sender, FileSystemEventArgs e)
    {
        AddDocument(GetPath(e.FullPath));
    }

    private void HandleChanged(object sender, FileSystemEventArgs e)
    {
        EditDocument(GetPath(e.FullPath));
    }

    private void HandleRenamed(object sender, RenamedEventArgs e)
    {
        MoveDocument(GetPath(e.OldFullPath), GetPath(e.FullPath));
    }

    private void HandleDeleted(object sender, FileSystemEventArgs e)
    {
        DeleteDocument(GetPath(e.FullPath));
    }

    private void HandleError(object sender, ErrorEventArgs e)
    {
        var exception = e.GetException();

        if (!_retry(exception))
            return;

        _fsw!.EnableRaisingEvents = false;

        _fsw.Deleted -= HandleDeleted;
        _fsw.Renamed -= HandleRenamed;
        _fsw.Changed -= HandleChanged;
        _fsw.Created -= HandleCreated;
        _fsw.Error -= HandleError;

        _fsw.Dispose();

        try
        {
            _fsw = CreateWatcher();
        }
        catch (Exception ex) when (ex is ArgumentException or FileNotFoundException)
        {
            // The workspace path has disappeared; nothing we can do at this point.
        }
    }
}
