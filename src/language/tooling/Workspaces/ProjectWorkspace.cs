using Vezel.Celerity.Language.Tooling.Projects;

namespace Vezel.Celerity.Language.Tooling.Workspaces;

public sealed class ProjectWorkspace : Workspace
{
    public const string ConfigurationFileName = "celerity.json";

    public ProjectConfiguration Configuration { get; }

    private readonly bool _disableAnalysis;

    private ProjectWorkspace(
        string path, SourceTextProvider textProvider, ProjectConfiguration configuration, bool disableAnalysis)
        : base(path, textProvider)
    {
        Configuration = configuration;
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
        var attrs = path == WorkspaceDocument.EntryPointPath
            ? WorkspaceDocumentAttributes.EntryPoint
            : WorkspaceDocumentAttributes.None;

        if (_disableAnalysis || System.IO.Path.GetDirectoryName(path) != Configuration.SourcePath)
            attrs |= WorkspaceDocumentAttributes.DisableAnalyzers | WorkspaceDocumentAttributes.SuppressDiagnostics;

        return attrs;
    }
}
