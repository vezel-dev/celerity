namespace Vezel.Celerity.Language.Tooling.Workspaces;

public abstract class Workspace
{
    public event Action<WorkspaceDocument>? DocumentAdded
    {
        add => _documentAdded.Add(value);
        remove => _documentAdded.Remove(value);
    }

    public event Action<WorkspaceDocument, WorkspaceDocument>? DocumentEdited
    {
        add => _documentEdited.Add(value);
        remove => _documentEdited.Remove(value);
    }

    public event Action<WorkspaceDocument, WorkspaceDocument>? DocumentRenamed
    {
        add => _documentRenamed.Add(value);
        remove => _documentRenamed.Remove(value);
    }

    public event Action<WorkspaceDocument>? DocumentRemoved
    {
        add => _documentRemoved.Add(value);
        remove => _documentRemoved.Remove(value);
    }

    public string Path { get; }

    public ImmutableDictionary<string, WorkspaceDocument> Documents { get; private set; } =
        ImmutableDictionary<string, WorkspaceDocument>.Empty;

    private readonly Event<WorkspaceDocument> _documentAdded = new();

    private readonly Event<WorkspaceDocument, WorkspaceDocument> _documentEdited = new();

    private readonly Event<WorkspaceDocument, WorkspaceDocument> _documentRenamed = new();

    private readonly Event<WorkspaceDocument> _documentRemoved = new();

    protected Workspace(string path)
    {
        Check.Null(path);

        Path = path;
    }

    public WorkspaceDocument? GetEntryPointDocument()
    {
        return Documents.Values.SingleOrDefault(
            static doc => doc.Attributes.HasFlag(WorkspaceDocumentAttributes.EntryPoint));
    }

    protected internal abstract ValueTask<SourceText> LoadTextAsync(string path, CancellationToken cancellationToken);

    protected internal abstract IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers();

    protected abstract WorkspaceDocumentAttributes GetDocumentAttributes(string path);

    private WorkspaceDocument CreateDocument(string path)
    {
        return new(this, GetDocumentAttributes(path), path);
    }

    // The following methods need to be resilient against a buggy WorkspaceWatcher. FileSystemWatcher, for example, is
    // notoriously quirky, and we do not want WorkspaceWatcher implementations to have to deal with that.
    //
    // Note that WorkspaceWatcher takes care of serializing concurrent document operations, so we do not need to lock.

    internal void AddOrEditDocument(string path)
    {
        if (Documents.TryGetValue(path, out var oldDoc))
        {
            var editedOldDoc = CreateDocument(path);

            Documents = Documents.SetItem(path, editedOldDoc);

            oldDoc.IsCurrent = false;

            _documentEdited.Raise(oldDoc, editedOldDoc);
        }

        var newDoc = CreateDocument(path);

        Documents = Documents.Add(path, newDoc);

        _documentAdded.Raise(newDoc);
    }

    internal void MoveDocument(string oldPath, string newPath)
    {
        var oldDoc = Documents.GetValueOrDefault(oldPath);
        var newDoc = Documents.GetValueOrDefault(newPath);

        switch ((oldDoc, newDoc))
        {
            case ({ }, null):
            {
                var movedOldDoc = CreateDocument(newPath);

                Documents = Documents.Remove(oldPath).Add(newPath, movedOldDoc);

                oldDoc.IsCurrent = false;

                _documentRenamed.Raise(oldDoc, movedOldDoc);

                break;
            }

            case (null, { }):
            {
                var editedNewDoc = CreateDocument(newPath);

                Documents = Documents.SetItem(newPath, editedNewDoc);

                newDoc.IsCurrent = false;

                _documentEdited.Raise(newDoc, editedNewDoc);

                break;
            }

            case ({ }, { }):
            {
                var editedOldDoc = CreateDocument(oldPath);
                var editedNewDoc = CreateDocument(newPath);

                Documents = Documents.SetItem(oldPath, editedOldDoc).SetItem(newPath, editedNewDoc);

                oldDoc.IsCurrent = false;
                newDoc.IsCurrent = false;

                _documentEdited.Raise(oldDoc, editedOldDoc);
                _documentEdited.Raise(newDoc, editedNewDoc);

                break;
            }
        }
    }

    internal void DeleteDocument(string path)
    {
        if (!Documents.TryGetValue(path, out var doc))
            return;

        Documents = Documents.Remove(path);

        doc.IsCurrent = false;

        _documentRemoved.Raise(doc);
    }

    internal void ClearDocuments()
    {
        var docs = Documents;

        Documents = ImmutableDictionary<string, WorkspaceDocument>.Empty;

        foreach (var (_, doc) in docs)
            doc.IsCurrent = false;
    }
}
