namespace Vezel.Celerity.Language.Tooling.Projects;

public sealed class ProjectConfiguration
{
    public const string DefaultFileName = "celerity.json";

    public string Name { get; }

    public ProjectKind Kind { get; }

    public string SourcePath { get; }

    public ImmutableDictionary<ModulePath, string> SearchPaths { get; }

    public string? License { get; }

    // TODO: https://github.com/dotnet/runtime/issues/19317
    public string Version { get; }

    public ImmutableArray<LintPass> LintPasses { get; }

    public LintConfiguration LintConfiguration { get; }

    public ImmutableArray<DiagnosticAnalyzer> DiagnosticAnalyzers { get; }

    private ProjectConfiguration(
        string name,
        ProjectKind kind,
        string sourcePath,
        ImmutableDictionary<ModulePath, string> searchPaths,
        string? license,
        string version,
        ImmutableArray<LintPass> lintPasses,
        LintConfiguration lintConfiguration,
        ImmutableArray<DiagnosticAnalyzer> diagnosticAnalyzers)
    {
        Name = name;
        Kind = kind;
        SourcePath = sourcePath;
        SearchPaths = searchPaths;
        License = license;
        Version = version;
        LintPasses = lintPasses;
        LintConfiguration = lintConfiguration;
        DiagnosticAnalyzers = diagnosticAnalyzers;
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public static async ValueTask<ProjectConfiguration> LoadAsync(
        Stream stream, CancellationToken cancellationToken = default)
    {
        using var json = await JsonDocument.ParseAsync(
            stream,
            new()
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip,
            },
            cancellationToken).ConfigureAwait(false);

        [DoesNotReturn]
        static void Error(string message)
        {
            throw new ProjectException(message);
        }

        var root = json.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
            Error("Root element must be an object.");

        if (!root.TryGetProperty("name"u8, out var nameProp))
            Error("'name' property is missing.");

        if (nameProp.ValueKind != JsonValueKind.String)
            Error("'name' property must be a string.");

        var kind = ProjectKind.Program;

        if (root.TryGetProperty("kind"u8, out var kindProp))
        {
            if (kindProp.ValueEquals("library"u8))
                kind = ProjectKind.Library;
            else if (!kindProp.ValueEquals("program"u8))
                Error("'kind' property, if present, must be 'program' (default) or 'library'.");
        }

        var path = "src";

        if (root.TryGetProperty("path"u8, out var pathProp))
        {
            if (pathProp.ValueKind != JsonValueKind.String)
                Error("'path' property, if present, must be a string.");

            path = Path.TrimEndingDirectorySeparator(pathProp.GetString()!);

            if (Path.IsPathFullyQualified(path))
                Error("'path' property, if present, must be relative.");

            // TODO: It would be good to verify that the path does not contain any . or .. segments.
        }

        var paths = ImmutableDictionary.CreateBuilder<ModulePath, string>();

        if (root.TryGetProperty("paths"u8, out var pathsProp))
        {
            if (pathsProp.ValueKind != JsonValueKind.Object)
                Error("'paths' property, if present, must be an object.");

            foreach (var prop in pathsProp.EnumerateObject())
            {
                if (!ModulePath.TryCreate(prop.Name, out var modPath))
                    Error($"Module path '{prop.Name}' is invalid.");

                if (paths.ContainsKey(modPath))
                    Error($"Module path '{prop.Name}' has multiple entries.");

                var value = prop.Value;

                if (value.ValueKind != JsonValueKind.String)
                    Error($"Directory path for module path '{prop.Name}' must be a string.");

                var dir = Path.TrimEndingDirectorySeparator(value.GetString()!);

                if (Path.IsPathFullyQualified(dir))
                    Error($"Directory path for module path '{prop.Name}' must be relative.");

                // TODO: It would be good to verify that the path does not contain any . or .. segments.

                paths[modPath] = dir;
            }
        }

        var license = default(string);

        if (root.TryGetProperty("license"u8, out var licenseProp))
        {
            if (licenseProp.ValueKind != JsonValueKind.String)
                Error("'license' property, if present, must be a string.");

            license = licenseProp.GetString()!;

            if (!SpdxExpression.IsValidExpression(license, SpdxLicenseOptions.Relaxed))
                Error("'license' property, if present, must be a valid SPDX license expression.");
        }

        var version = "0.0.0";

        if (root.TryGetProperty("version"u8, out var versionProp))
        {
            if (versionProp.ValueKind != JsonValueKind.String)
                Error("'version' property, if present, must be a string.");

            if (!SemanticVersion.TryParse(versionProp.GetString()!, out var semVer))
                Error("'version' property, if present, must be a valid Semantic Versioning 2.0.0 version number.");

            version = semVer.ToFullString();
        }

        var passes = LintPass.DefaultPasses.ToBuilder();
        var severities = LintConfiguration.Default;

        if (root.TryGetProperty("lints"u8, out var lintsProp))
        {
            if (lintsProp.ValueKind != JsonValueKind.Object)
                Error("'lints' property, if present, must be an object.");

            var allPasses = LintPass.DefaultPasses.ToDictionary(static pass => pass.Code);

            foreach (var prop in lintsProp.EnumerateObject())
            {
                if (!DiagnosticCode.TryCreate(prop.Name, out var code) || !allPasses.TryGetValue(code, out var pass))
                    Error($"Lint name '{prop.Name}' is invalid.");

                if (severities.TryGetSeverity(code, out _))
                    Error($"Lint name '{prop.Name}' has multiple entries.");

                var value = prop.Value;

                if (value.ValueKind == JsonValueKind.Null)
                {
                    _ = passes.Remove(pass);

                    continue;
                }

                DiagnosticSeverity severity;

                if (value.ValueEquals("none"u8))
                    severity = DiagnosticSeverity.None;
                else if (value.ValueEquals("warning"u8))
                    severity = DiagnosticSeverity.Warning;
                else if (value.ValueEquals("error"u8))
                    severity = DiagnosticSeverity.Error;
                else
                    Error($"Lint severity for '{prop.Name}' must be null, 'none', 'warning', or 'error'.");

                severities = severities.WithSeverity(code, severity);
            }
        }

        return new(
            nameProp.GetString()!,
            kind,
            path,
            paths.ToImmutable(),
            license,
            version,
            passes.DrainToImmutable(),
            severities,
            ImmutableArray.Create<DiagnosticAnalyzer>(new LintDiagnosticAnalyzer(passes, severities)));
    }
}
