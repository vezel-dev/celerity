// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Language.Tooling.Classification;

namespace Vezel.Celerity.Language.Tooling.Workspaces;

public sealed class WorkspaceDocument
{
    public const string EntryPointPath = "main.cel";

    public Workspace Workspace { get; }

    public WorkspaceDocumentAttributes Attributes { get; }

    public string Path => _state switch
    {
        SourceText text => text.Path,
        SyntaxTree syntax => syntax.Path,
        SemanticTree semantics => semantics.Syntax.Path,
        var path => Unsafe.As<string>(path),
    };

    // This starts out as a string (path). As callers access text, syntax, and semantic information, it gets promoted to
    // SourceText, SyntaxTree, and SemanticTree, respectively. SemanticTree links back to SyntaxTree which in turn
    // soft-links back to SourceText. The SourceText can always be reconstructed from the SyntaxTree (and by extension
    // SemanticTree). This design allows us to lazily add more information as needed, while dropping the source text
    // when the GC decides that doing so makes sense.
    private object _state;

    private IEnumerable<Diagnostic>? _diagnostics;

    private IEnumerable<ClassifiedSourceTextSpan>? _classifications;

    internal WorkspaceDocument(Workspace workspace, WorkspaceDocumentAttributes attributes, string path)
    {
        Workspace = workspace;
        Attributes = attributes;
        _state = path;

        if (attributes.HasFlag(WorkspaceDocumentAttributes.SuppressDiagnostics))
            _diagnostics = [];
    }

    public ValueTask<SourceText> GetTextAsync(CancellationToken cancellationToken = default)
    {
        return GetTextAsync();

        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
        async ValueTask<SourceText> GetTextAsync()
        {
            switch (_state)
            {
                case SourceText text:
                    return text;
                case SyntaxTree syntax:
                    return syntax.GetText();
                case SemanticTree semantics:
                    return semantics.Syntax.GetText();
                case var path:
                    var state = await Workspace.TextProvider.GetTextAsync(
                        Workspace, Unsafe.As<string>(path), cancellationToken).ConfigureAwait(false);

                    Check.Operation(state != null);

                    _state = state;

                    return state;
            }
        }
    }

    public ValueTask<SyntaxTree> GetSyntaxAsync(CancellationToken cancellationToken = default)
    {
        return GetSyntaxAsync();

        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
        async ValueTask<SyntaxTree> GetSyntaxAsync()
        {
            switch (_state)
            {
                case SyntaxTree syntax:
                    return syntax;
                case SemanticTree semantics:
                    return semantics.Syntax;
                default:
                    var state = SyntaxTree.Parse(
                        await GetTextAsync(cancellationToken).ConfigureAwait(false), SyntaxMode.Module);

                    _state = state;

                    return state;
            }
        }
    }

    public ValueTask<SemanticTree> GetSemanticsAsync(CancellationToken cancellationToken = default)
    {
        return GetSemanticsAsync();

        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
        async ValueTask<SemanticTree> GetSemanticsAsync()
        {
            if (_state is not SemanticTree semantics)
            {
                var analyzers = Array.Empty<DiagnosticAnalyzer>().AsEnumerable();

                if (!Attributes.HasFlag(WorkspaceDocumentAttributes.DisableAnalyzers))
                {
                    analyzers = Workspace.GetDiagnosticAnalyzers();

                    Check.Operation(analyzers != null && analyzers.All(static analyzer => analyzer != null));
                }

                semantics = SemanticTree.Analyze(
                    await GetSyntaxAsync(cancellationToken).ConfigureAwait(false), context: null, analyzers);

                _state = semantics;
            }

            return semantics;
        }
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public async ValueTask<IEnumerable<Diagnostic>> GetDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        if (_diagnostics == null)
        {
            var semantics = await GetSemanticsAsync(cancellationToken).ConfigureAwait(false);

            _diagnostics = [.. semantics
                .Syntax
                .Diagnostics
                .Concat(semantics.Diagnostics)
                .Where(static diag => diag.Severity != DiagnosticSeverity.None)
                .OrderBy(static diag => diag.Span)];
        }

        return _diagnostics;
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public async ValueTask<IEnumerable<ClassifiedSourceTextSpan>> GetClassificationsAsync(
        CancellationToken cancellationToken = default)
    {
        if (_classifications == null)
        {
            var root = (await GetSemanticsAsync(cancellationToken).ConfigureAwait(false)).Root;

            _classifications = TextClassifier.ClassifySemantically(root, root.Syntax.FullSpan);
        }

        return _classifications;
    }
}
