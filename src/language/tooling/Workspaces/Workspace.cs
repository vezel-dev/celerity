// SPDX-License-Identifier: 0BSD

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

    public SourceTextProvider TextProvider { get; }

    public ImmutableDictionary<string, WorkspaceDocument> Documents => _documents;

    private readonly Event<WorkspaceDocument> _documentAdded = new();

    private readonly Event<WorkspaceDocument, WorkspaceDocument> _documentEdited = new();

    private readonly Event<WorkspaceDocument, WorkspaceDocument> _documentRenamed = new();

    private readonly Event<WorkspaceDocument> _documentRemoved = new();

    // This is always modified under the WorkspaceWatcher lock, but it may be read by anyone without locking. The
    // volatile ensures that a fresh reference will always be seen by readers.
    private volatile ImmutableDictionary<string, WorkspaceDocument> _documents =
        ImmutableDictionary<string, WorkspaceDocument>.Empty;

    protected Workspace(string path, SourceTextProvider textProvider)
    {
        Check.Null(path);
        Check.Null(textProvider);

        Path = path;
        TextProvider = textProvider;
    }

    public WorkspaceDocument? GetEntryPointDocument()
    {
        return _documents.Values.SingleOrDefault(
            static doc => doc.Attributes.HasFlag(WorkspaceDocumentAttributes.EntryPoint));
    }

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
        if (_documents.TryGetValue(path, out var oldDoc))
        {
            var editedOldDoc = CreateDocument(path);

            _documents = _documents.SetItem(path, editedOldDoc);

            _documentEdited.Raise(oldDoc, editedOldDoc);
        }

        var newDoc = CreateDocument(path);

        _documents = _documents.Add(path, newDoc);

        _documentAdded.Raise(newDoc);
    }

    internal void MoveDocument(string oldPath, string newPath)
    {
        var oldDoc = _documents.GetValueOrDefault(oldPath);
        var newDoc = _documents.GetValueOrDefault(newPath);

        switch ((oldDoc, newDoc))
        {
            case ({ }, null):
            {
                var movedOldDoc = CreateDocument(newPath);

                _documents = _documents.Remove(oldPath).Add(newPath, movedOldDoc);

                _documentRenamed.Raise(oldDoc, movedOldDoc);

                break;
            }

            case (null, { }):
            {
                var editedNewDoc = CreateDocument(newPath);

                _documents = _documents.SetItem(newPath, editedNewDoc);

                _documentEdited.Raise(newDoc, editedNewDoc);

                break;
            }

            case ({ }, { }):
            {
                var editedOldDoc = CreateDocument(oldPath);
                var editedNewDoc = CreateDocument(newPath);

                _documents = _documents.SetItem(oldPath, editedOldDoc).SetItem(newPath, editedNewDoc);

                _documentEdited.Raise(oldDoc, editedOldDoc);
                _documentEdited.Raise(newDoc, editedNewDoc);

                break;
            }
        }
    }

    internal void DeleteDocument(string path)
    {
        if (!_documents.TryGetValue(path, out var doc))
            return;

        _documents = _documents.Remove(path);

        _documentRemoved.Raise(doc);
    }

    internal void ClearDocuments()
    {
        _documents = _documents.Clear();
    }
}
