using Vezel.Celerity.Language.Tooling.Projects;

namespace Vezel.Celerity.Language.Tooling.Workspaces;

public sealed class ProjectWorkspace : Workspace
{
    public const string ConfigurationFileName = "celerity.json";

    public ProjectConfiguration Configuration { get; }

    private readonly DirectoryInfo _sourceDirectory;

    private readonly bool _disableAnalysis;

    private ProjectWorkspace(
        string path, SourceTextProvider textProvider, ProjectConfiguration configuration, bool disableAnalysis)
        : base(path, textProvider)
    {
        Configuration = configuration;
        _sourceDirectory = new(System.IO.Path.Join(path, configuration.SourcePath));
        _disableAnalysis = disableAnalysis;
    }

    public static ValueTask<ProjectWorkspace> OpenAsync(
        string path, SourceTextProvider textProvider, CancellationToken cancellationToken = default)
    {
        return OpenAsync(path, textProvider, false, cancellationToken);
    }

    public static ValueTask<ProjectWorkspace> OpenAsync(
        string path, SourceTextProvider textProvider, bool disableAnalysis, CancellationToken cancellationToken = default)
    {
        Check.Null(path);
        Check.Null(textProvider);

        return OpenAsync();

        [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
        [SuppressMessage("", "CA2000")] // TODO: https://github.com/dotnet/roslyn-analyzers/issues/6512
        async ValueTask<ProjectWorkspace> OpenAsync()
        {
            var stream = File.OpenRead(System.IO.Path.Join(path, ConfigurationFileName));

            ProjectConfiguration cfg;

            await using (stream.ConfigureAwait(false))
                cfg = await ProjectConfiguration.LoadAsync(stream, cancellationToken).ConfigureAwait(false);

            return new(path, textProvider, cfg, disableAnalysis);
        }
    }

    protected internal override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
    {
        return Configuration.DiagnosticAnalyzers;
    }

    protected override WorkspaceDocumentAttributes GetDocumentAttributes(string path)
    {
        var file = new FileInfo(System.IO.Path.Join(Path, path));
        var srcAttrs = _disableAnalysis
            ? WorkspaceDocumentAttributes.DisableAnalyzers | WorkspaceDocumentAttributes.SuppressDiagnostics
            : WorkspaceDocumentAttributes.None;

        if ((file.DirectoryName, file.Name) == (_sourceDirectory.FullName, WorkspaceDocument.EntryPointPath))
            return WorkspaceDocumentAttributes.EntryPoint | srcAttrs;

        var current = file.Directory;

        while (current != null)
        {
            // Is the document within the configured source directory?
            if (current.FullName == _sourceDirectory.FullName)
                return srcAttrs;

            current = current.Parent;
        }

        // It must be a loose document or a dependency document, so no analyzers and diagnostics.
        return WorkspaceDocumentAttributes.DisableAnalyzers | WorkspaceDocumentAttributes.SuppressDiagnostics;
    }
}
