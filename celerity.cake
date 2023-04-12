#addin nuget:?package=Cake.Npm&version=2.0.0
#addin nuget:?package=Cake.Npx&version=1.7.0

#nullable enable

private const string RootProject = "celerity.proj";

private const string BenchmarksProject = "src/benchmarks/benchmarks.csproj";

private const string DriverProject = "src/driver/driver.csproj";

private const string LibraryDirectory = "src/language/library";

private const string ExtensionDirectory = "src/extensions/vscode";

private const string DocumentationDirectory = "doc";

private const string OutputPath = "out";

private const string GitHubPackageGlob = $"{OutputPath}/pkg/debug/*.nupkg";

private const string NuGetPackageGlob = $"{OutputPath}/pkg/release/*.nupkg";

private const string ExtensionPackageGlob = $"{OutputPath}/pkg/*.vsix";

private readonly var _target = Argument("t", "Default");

private readonly var _configuration = Argument("c", "Debug");

private readonly var _filter = Argument("f", default(string));

private readonly var _githubKey = Argument("github-key", default(string));

private readonly var _nugetKey = Argument("nuget-key", default(string));

private readonly var _marketplaceKey = Argument("marketplace-key", default(string));

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
            FileName = System.IO.Path.Join(OutputPath, "log", name),
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

        Information("Restoring {0}...", $"{ExtensionDirectory}/package.json");
        NpmInstall(
            new NpmInstallSettings()
            {
                WorkingDirectory = ExtensionDirectory,
            });
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

        Information("Checking {0}", $"{LibraryDirectory}/celerity.json");
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

        Information("Building {0}...", $"{ExtensionDirectory}/package.json");
        NpmRunScript(
            new NpmRunScriptSettings()
            {
                WorkingDirectory = ExtensionDirectory,
                ScriptName = "build",
            });

        Information("Checking {0}...", DocumentationDirectory);
        Npx(
            "markdownlint-cli2",
            settings => settings.WorkingDirectory = "doc");
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

        Information("Testing {0}...", BenchmarksProject);
        DotNetRun(
            BenchmarksProject,
            new ProcessArgumentBuilder()
                .Append("-t"),
            new()
            {
                Configuration = _configuration,
                NoBuild = true,
            });

        Information("Testing {0}...", $"{LibraryDirectory}/celerity.json");
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
        Information("Benchmarking {0}...", BenchmarksProject);
        DotNetRun(
            BenchmarksProject,
            _filter != null
                ? new ProcessArgumentBuilder()
                    .AppendSwitchQuoted("-f", _filter)
                : null,
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

        Information("Packing {0}...", $"{ExtensionDirectory}/package.json");
        NpmRunScript(
            new NpmRunScriptSettings()
            {
                WorkingDirectory = ExtensionDirectory,
                ScriptName = "pack",
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

        Information("Cleaning {0}...", $"{ExtensionDirectory}/package.json");
        NpmRunScript(
            new NpmRunScriptSettings()
            {
                WorkingDirectory = ExtensionDirectory,
                ScriptName = "clean",
            });
    });

Task("Prune")
    .Does(() =>
    {
        void Prune(string directory, string extension)
        {
            var glob = $"{OutputPath}/{directory}/**/*.{extension}";

            Information("Pruning {0}...", glob);
            DeleteFiles(glob);
        }

        Prune("log", "binlog");
        Prune("trx", "trx");
        Prune("pkg", "nupkg");
        Prune("pkg", "vsix");
    });

Task("Upload-GitHub")
    .WithCriteria(BuildSystem.GitHubActions.Environment.Workflow.Ref == "refs/heads/master")
    .WithCriteria(_configuration == "Debug")
    .IsDependentOn("Pack")
    .Does(() =>
    {
        Information("Pushing {0} to GitHub...", GitHubPackageGlob);
        DotNetTool(
            null,
            "gpr push",
            new ProcessArgumentBuilder()
                .AppendQuoted(GitHubPackageGlob)
                .AppendSwitchQuotedSecret("-k", _githubKey));
    });

Task("Upload-NuGet")
    .WithCriteria(BuildSystem.GitHubActions.Environment.Workflow.Ref.StartsWith("refs/tags/v"))
    .WithCriteria(_configuration == "Release")
    .IsDependentOn("Pack")
    .Does(() =>
    {
        Information("Pushing {0} to NuGet...", NuGetPackageGlob);
        DotNetNuGetPush(
            NuGetPackageGlob,
            new()
            {
                Source = "https://api.nuget.org/v3/index.json",
                ApiKey = _nugetKey,
            });
    });

Task("Upload-Marketplace")
    .WithCriteria(BuildSystem.GitHubActions.Environment.Workflow.Ref.StartsWith("refs/tags/v"))
    .WithCriteria(_configuration == "Release")
    .IsDependentOn("Pack")
    .Does(() =>
    {
        var files = GetFiles(ExtensionPackageGlob);
        var cwd = new DirectoryPath(Environment.CurrentDirectory);
        var args = new ProcessArgumentBuilder()
            .Append("--pre-release")
            .AppendSwitchQuotedSecret("-p", _marketplaceKey);

        foreach (var file in files)
            _ = args.AppendSwitchQuoted("-i", file.FullPath);

        Information("Pushing {0} to Visual Studio Marketplace...", ExtensionPackageGlob);
        Npx(
            "vsce publish",
            args,
            settings => settings.WorkingDirectory = ExtensionDirectory);
    });

RunTarget(_target);
