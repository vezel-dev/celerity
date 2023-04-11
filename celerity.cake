private const string RootProject = "celerity.proj";

private const string BenchmarksDirectory = "src/benchmarks";

private const string DriverProject = "src/driver/driver.csproj";

private const string LibraryDirectory = "src/language/library";

private const string PackagesGlob = "pkg/feed/*.nupkg";

private readonly var _target = Argument("t", "Default");

private readonly var _configuration = Argument("c", "Debug");

private readonly var _filter = Argument("f", default(string));

private readonly var _key = Argument("k", default(string));

Task("Default")
    .IsDependentOn("Test")
    .IsDependentOn("Pack");

Task("Restore")
    .Does(() =>
    {
        Information("Restoring {0}...", RootProject);
        DotNetRestore(RootProject);
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        Information("Building {0}...", RootProject);
        DotNetBuild(
            RootProject,
            new()
            {
                MSBuildSettings = new()
                {
                    ConsoleLoggerSettings = new()
                    {
                        NoSummary = true,
                    },
                },
                NoLogo = true,
                Configuration = _configuration,
            });
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        Information("Testing {0}...", RootProject);
        DotNetTest(
            RootProject,
            new()
            {
                MSBuildSettings = new()
                {
                    ConsoleLoggerSettings = new()
                    {
                        NoSummary = true,
                    },
                },
                NoLogo = true,
                Verbosity = DotNetVerbosity.Normal,
                Configuration = _configuration,
                NoBuild = true,
            });

        Information("Testing {0}...", BenchmarksDirectory);
        DotNetRun(
            BenchmarksDirectory,
            new ProcessArgumentBuilder()
                .Append("-t"),
            new()
            {
                Configuration = _configuration,
                NoBuild = true,
            });

        Information("Testing {0}...", LibraryDirectory);
        DotNetRun(
            DriverProject,
            new ProcessArgumentBuilder()
                .Append("test")
                .AppendSwitchQuoted("-w", LibraryDirectory),
            new()
            {
                Configuration = _configuration,
                NoBuild = true,
            });
    });

Task("Benchmark")
    .IsDependentOn("Build")
    .Does(() =>
    {
        Information("Running {0}...", BenchmarksDirectory);
        DotNetRun(
            BenchmarksDirectory,
            _filter != null ? new ProcessArgumentBuilder().AppendSwitchQuoted("-f", _filter) : null,
            new()
            {
                Configuration = _configuration,
                NoBuild = true,
            });
    });

Task("Publish")
    .IsDependentOn("Build")
    .Does(() =>
    {
        Information("Publishing {0}...", RootProject);
        DotNetPublish(
            RootProject,
            new()
            {
                NoLogo = true,
                Configuration = _configuration,
                NoBuild = true,
            });

        Information("Checking {0}", LibraryDirectory);
        DotNetRun(
            DriverProject,
            new ProcessArgumentBuilder()
                .Append("check")
                .AppendSwitchQuoted("-w", LibraryDirectory),
            new()
            {
                Configuration = _configuration,
                NoBuild = true,
            });
        DotNetRun(
            DriverProject,
            new ProcessArgumentBuilder()
                .Append("format")
                .AppendSwitchQuoted("-w", LibraryDirectory),
            new()
            {
                Configuration = _configuration,
                NoBuild = true,
            });
    });

Task("Pack")
    .IsDependentOn("Publish")
    .Does(() =>
    {
        Information("Packing {0}...", RootProject);
        DotNetPack(
            RootProject,
            new()
            {
                NoLogo = true,
                Configuration = _configuration,
                NoBuild = true,
            });
    });

Task("Clean")
    .Does(() =>
    {
        Information("Cleaning {0}...", RootProject);
        DotNetClean(
            RootProject,
            new()
            {
                NoLogo = true,
                Configuration = _configuration,
            });
    });

Task("Package")
    .WithCriteria(BuildSystem.GitHubActions.Environment.Workflow.Ref == "refs/heads/master")
    .IsDependentOn("Pack")
    .Does(() =>
    {
        Information("Pushing {0} to GitHub...", PackagesGlob);
        DotNetTool(
            null,
            "gpr push",
            new ProcessArgumentBuilder()
                .AppendQuoted(PackagesGlob)
                .AppendSwitchQuotedSecret("-k", _key));
    });

Task("Release")
    .WithCriteria(BuildSystem.GitHubActions.Environment.Workflow.Ref.StartsWith("refs/tags/v"))
    .IsDependentOn("Pack")
    .Does(() =>
    {
        Information("Pushing {0} to NuGet...", PackagesGlob);
        DotNetNuGetPush(
            PackagesGlob,
            new()
            {
                Source = "https://api.nuget.org/v3/index.json",
                ApiKey = _key,
            });
    });

RunTarget(_target);
