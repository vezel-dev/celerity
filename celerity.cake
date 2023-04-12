#addin nuget:?package=Cake.Npm&version=2.0.0
#addin nuget:?package=Cake.Npx&version=1.7.0

private const string RootProject = "celerity.proj";

private const string BenchmarksDirectory = "src/benchmarks";

private const string DriverProject = "src/driver/driver.csproj";

private const string LibraryDirectory = "src/language/library";

private const string DocumentationGlob = "doc/**/*.md";

private const string PackageGlob = "out/pkg/debug/*.nupkg";

private const string ReleaseGlob = "out/pkg/release/*.nupkg";

private readonly var _target = Argument("t", "Default");

private readonly var _configuration = Argument("c", "Debug");

private readonly var _filter = Argument("f", default(string));

private readonly var _key = Argument("k", default(string));

private static DotNetMSBuildSettings ConfigureMSBuild(string target)
{
    var prefix = $"{target}_{Environment.UserName}_{Environment.MachineName}_";
    var time = DateTime.Now;

    string name;

    do
    {
        name = $"{prefix}{time:yyyy-MM-dd_HH_mm_ss}.binlog";
        time = time.AddSeconds(1);
    }
    while (System.IO.File.Exists(name));

    return new()
    {
        NoLogo = true,
        BinaryLogger = new()
        {
            Enabled = true,
            FileName = System.IO.Path.Join("out", "log", name),
        },
        ConsoleLoggerSettings = new()
        {
            NoSummary = true,
        },
        TreatAllWarningsAs = MSBuildTreatAllWarningsAs.Error,
        // TODO: https://github.com/cake-build/cake/issues/4144
        ArgumentCustomization = args =>
            args
                .Append("-nr:false")
                .Append("-ds:false"),
    };
}

Task("Default")
    .IsDependentOn("Test")
    .IsDependentOn("Pack");

Task("Restore")
    .Does(() =>
    {
        Information("Restoring {0}...", RootProject);
        DotNetRestore(
            RootProject,
            new()
            {
                MSBuildSettings = ConfigureMSBuild("restore"),
            });

        Information("Restoring {0}...", "package.json");
        NpmInstall();
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
                MSBuildSettings = ConfigureMSBuild("build"),
                Configuration = _configuration,
                NoRestore = true,
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

        Information("Checking {0}...", DocumentationGlob);
        Npx(
            "markdownlint-cli2",
            new ProcessArgumentBuilder()
                .AppendQuoted(DocumentationGlob));
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
                MSBuildSettings = ConfigureMSBuild("test"),
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
                MSBuildSettings = ConfigureMSBuild("publish"),
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
                MSBuildSettings = ConfigureMSBuild("pack"),
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
                MSBuildSettings = ConfigureMSBuild("clean"),
                Configuration = _configuration,
            });
    });

Task("Package")
    .WithCriteria(BuildSystem.GitHubActions.Environment.Workflow.Ref == "refs/heads/master")
    .WithCriteria(_configuration == "Debug")
    .IsDependentOn("Pack")
    .Does(() =>
    {
        Information("Pushing {0} to GitHub...", PackageGlob);
        DotNetTool(
            null,
            "gpr push",
            new ProcessArgumentBuilder()
                .AppendQuoted(PackageGlob)
                .AppendSwitchQuotedSecret("-k", _key));
    });

Task("Release")
    .WithCriteria(BuildSystem.GitHubActions.Environment.Workflow.Ref.StartsWith("refs/tags/v"))
    .WithCriteria(_configuration == "Release")
    .IsDependentOn("Pack")
    .Does(() =>
    {
        Information("Pushing {0} to NuGet...", ReleaseGlob);
        DotNetNuGetPush(
            ReleaseGlob,
            new()
            {
                Source = "https://api.nuget.org/v3/index.json",
                ApiKey = _key,
            });
    });

RunTarget(_target);
