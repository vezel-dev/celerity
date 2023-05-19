namespace Vezel.Celerity.Language.Tooling.Projects;

public sealed class SolutionConfiguration
{
    public ImmutableArray<string> Projects { get; }

    private SolutionConfiguration(ImmutableArray<string> projects)
    {
        Projects = projects;
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public static async ValueTask<SolutionConfiguration> LoadAsync(
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
            throw new SolutionException(message);
        }

        var root = json.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
            Error("Root element must be an object.");

        var projects = ImmutableArray.CreateBuilder<string>();
        var projectsSet = new HashSet<string>();

        if (root.TryGetProperty("projects"u8, out var projectsProp))
        {
            if (projectsProp.ValueKind != JsonValueKind.Array)
                Error("'projects' property, if present, must be an array.");

            foreach (var elem in projectsProp.EnumerateArray())
            {
                if (elem.ValueKind != JsonValueKind.String)
                    Error("Directory paths in 'projects' array must be strings.");

                var dirPath = elem.GetString()!;
                var dir = Path.TrimEndingDirectorySeparator(dirPath);

                if (Path.IsPathFullyQualified(dir))
                    Error($"Directory path '{dirPath}' in 'projects' array must be relative.");

                // TODO: It would be good to verify that the path does not contain any . or .. segments.

                if (!projectsSet.Add(dir))
                    Error($"Directory path '{dirPath}' in 'projects' array has multiple entries.");

                projects.Add(dir);
            }
        }

        return new(projects.DrainToImmutable());
    }
}
