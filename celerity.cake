#addin nuget:?package=Cake.DoInDirectory&version=6.0.0
#addin nuget:?package=Cake.GitVersioning&version=3.5.119
#addin nuget:?package=Cake.Npm&version=2.0.0
#addin nuget:?package=Cake.Npx&version=1.7.0

#nullable enable

// Arguments

var target = Argument("t", "default");
var configuration = Argument("c", "Debug");
var filter = Argument("f", default(string));

// Environment

var githubToken = EnvironmentVariable("GITHUB_TOKEN");
var nugetToken = EnvironmentVariable("NUGET_TOKEN");
var vsceToken = EnvironmentVariable("VSCE_TOKEN");
var ovsxToken = EnvironmentVariable("OVSX_TOKEN");

// Paths

var root = Context.Environment.WorkingDirectory;
var celerityProj = root.CombineWithFilePath("celerity.proj");

var doc = root.Combine("doc");

var src = root.Combine("src");
var srcLanguageLibrary = src.Combine("language").Combine("library");
var srcExtensionsVscode = src.Combine("extensions").Combine("vscode");
var driverCsproj = src.Combine("driver").CombineWithFilePath("driver.csproj");
var benchmarksCsproj = src.Combine("benchmarks").CombineWithFilePath("benchmarks.csproj");
var testsCsproj = src.Combine("tests").CombineWithFilePath("tests.csproj");
var trimmingCsproj = src.Combine("trimming").CombineWithFilePath("trimming.csproj");

var @out = root.Combine("out");
var outLogDotnet = @out.Combine("log").Combine("dotnet");
var outPkg = @out.Combine("pkg");
var outPkgDotnet = outPkg.Combine("dotnet");
var outPkgVscode = outPkg.Combine("vscode");

// Globs

var githubGlob = new GlobPattern(outPkgDotnet.Combine("debug").CombineWithFilePath("*.nupkg").FullPath);
var nugetGlob = new GlobPattern(outPkgDotnet.Combine("release").CombineWithFilePath("*.nupkg").FullPath);
var vscodeGlob = new GlobPattern(outPkgVscode.CombineWithFilePath("*.vsix").FullPath);

// Utilities

DotNetMSBuildSettings ConfigureMSBuild(string target)
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
            FileName = outLogDotnet.CombineWithFilePath(name).FullPath,
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

void NpmInstall(DirectoryPath directory)
{
    DoInDirectory(directory, NpmInstall);
}

void DotNetRun(FilePath project, Func<ProcessArgumentBuilder, ProcessArgumentBuilder> appender)
{
    DotNetRun(
        project.FullPath,
        appender(new()),
        new()
        {
            Configuration = configuration,
            NoBuild = true,
        });
}

void RunVSCode(Func<ProcessArgumentBuilder, ProcessArgumentBuilder> appender)
{
    var tool = Context.Tools.Resolve(new[] { "code.cmd", "code" }) ??
        throw new CakeException("code: Could not locate executable.");
    var code = StartProcess(tool, appender(new()).Render());

    if (code != 0)
        throw new CakeException(code, $"code: Process returned an error (exit code {code}).");
}

void UploadVSCode(string command, string token)
{
    var args = new ProcessArgumentBuilder()
        .Append("--pre-release")
        .Append("--skip-duplicate")
        .AppendSwitchQuotedSecret("-p", token);

    foreach (var file in GetFiles(vscodeGlob))
        _ = args.AppendSwitchQuoted("-i", file.FullPath);

    DoInDirectory(srcExtensionsVscode, () => Npx($"{command} publish", args));
}

// Tasks

Task("default")
    .IsDependentOn("test")
    .IsDependentOn("build")
    .IsDependentOn("pack");

Task("restore-dotnet")
    .Does(() =>
        DotNetRestore(
            celerityProj.FullPath,
            new()
            {
                MSBuildSettings = ConfigureMSBuild("restore"),
            }));

Task("restore-node")
    .Does(() => NpmInstall(root));

Task("restore-node-vscode")
    .IsDependentOn("restore-node")
    .Does(() => NpmInstall(srcExtensionsVscode));

Task("restore-node-doc")
    .IsDependentOn("restore-node")
    .Does(() => NpmInstall(doc));

Task("restore")
    .IsDependentOn("restore-dotnet")
    .IsDependentOn("restore-node-vscode")
    .IsDependentOn("restore-node-doc");

Task("build-dotnet")
    .IsDependentOn("restore-dotnet")
    .Does(() =>
        DotNetBuild(
            celerityProj.FullPath,
            new()
            {
                MSBuildSettings = ConfigureMSBuild("build"),
                Configuration = configuration,
                NoRestore = true,
            }));

Task("build-dotnet-trimming")
    .IsDependentOn("build-dotnet")
    .Does(() =>
        DotNetPublish(
            trimmingCsproj.FullPath,
            new()
            {
                MSBuildSettings = ConfigureMSBuild("publish"),
                Configuration = configuration,
                NoBuild = true,
            }));

Task("build-celerity-stdlib")
    .IsDependentOn("build-dotnet")
    .Does(() =>
        DoInDirectory(
            srcLanguageLibrary,
            () =>
            {
                foreach (var cmd in new[] { "check", "format" })
                    DotNetRun(driverCsproj.FullPath, args => args.Append(cmd));
            }));

Task("build-node-vscode")
    .IsDependentOn("restore-node-vscode")
    .Does(() => DoInDirectory(srcExtensionsVscode, () => NpmRunScript("build")));

Task("build-node-doc")
    .IsDependentOn("restore-node-doc")
    .Does(() => DoInDirectory(doc, () => Npx("markdownlint-cli2")));

Task("build")
    .IsDependentOn("build-dotnet-trimming")
    .IsDependentOn("build-celerity-stdlib")
    .IsDependentOn("build-node-vscode")
    .IsDependentOn("build-node-doc");

Task("pack-dotnet")
    .IsDependentOn("build-dotnet")
    .Does(() =>
        DotNetPack(
            celerityProj.FullPath,
            new()
            {
                MSBuildSettings = ConfigureMSBuild("pack"),
                Configuration = configuration,
                NoBuild = true,
            }));

Task("pack-node-vscode")
    .IsDependentOn("build-node-vscode")
    .Does(() => DoInDirectory(srcExtensionsVscode, () => NpmRunScript("pack")));

Task("pack")
    .IsDependentOn("pack-dotnet")
    .IsDependentOn("pack-node-vscode");

Task("test-dotnet-benchmarks")
    .IsDependentOn("build-dotnet")
    .Does(() => DotNetRun(benchmarksCsproj, args => args.Append("-t")));

Task("test-dotnet-tests")
    .IsDependentOn("build-dotnet")
    .Does(() =>
        DotNetTest(
            testsCsproj.FullPath,
            new()
            {
                MSBuildSettings = ConfigureMSBuild("test"),
                Configuration = configuration,
                NoBuild = true,
            }));

Task("test-celerity-stdlib")
    .IsDependentOn("build-celerity-stdlib")
    .Does(() => DotNetRun(driverCsproj.FullPath, args => args.Append("test")));

Task("test")
    .IsDependentOn("test-dotnet-benchmarks")
    .IsDependentOn("test-dotnet-tests")
    .IsDependentOn("test-celerity-stdlib");

Task("benchmark-dotnet-benchmarks")
    .WithCriteria(configuration == "Release")
    .IsDependentOn("build-dotnet")
    .Does(() =>
        DotNetRun(benchmarksCsproj, args => filter != null ? args.AppendSwitchQuoted("-f", filter) : args));

Task("benchmark")
    .IsDependentOn("benchmark-dotnet-benchmarks");

Task("install-node-vscode")
    .IsDependentOn("pack-node-vscode")
    .Does(() =>
    {
        var version = GitVersioningGetVersion(root.FullPath);

        RunVSCode(
            args => args.AppendSwitchQuoted(
                "--install-extension",
                outPkgVscode.CombineWithFilePath($"celerity-{version.NpmPackageVersion}.vsix").FullPath));
    });

Task("uninstall-node-vscode")
    .Does(() => RunVSCode(args => args.AppendSwitchQuoted("--uninstall-extension", "vezel.celerity")));

Task("upload-dotnet-github")
    .WithCriteria(BuildSystem.GitHubActions.Environment.Workflow.Ref == "refs/heads/master")
    .WithCriteria(configuration == "Debug")
    .IsDependentOn("pack-dotnet")
    .Does(() =>
        DotNetTool(
            null,
            "gpr push",
            new ProcessArgumentBuilder()
                .AppendQuoted(githubGlob)
                .AppendSwitchQuotedSecret("-k", githubToken)));

Task("upload-dotnet-nuget")
    .WithCriteria(BuildSystem.GitHubActions.Environment.Workflow.Ref.StartsWith("refs/tags/v"))
    .WithCriteria(configuration == "Release")
    .IsDependentOn("pack-dotnet")
    .Does(() =>
        DotNetNuGetPush(
            nugetGlob.Pattern,
            new()
            {
                Source = "https://api.nuget.org/v3/index.json",
                ApiKey = nugetToken,
                SkipDuplicate = true,
            }));

Task("upload-node-vscode-vsce")
    .WithCriteria(BuildSystem.GitHubActions.Environment.Workflow.Ref.StartsWith("refs/tags/v"))
    .WithCriteria(configuration == "Release")
    .IsDependentOn("pack-node-vscode")
    .Does(() => UploadVSCode("vsce", vsceToken));

Task("upload-node-vscode-ovsx")
    .WithCriteria(BuildSystem.GitHubActions.Environment.Workflow.Ref.StartsWith("refs/tags/v"))
    .WithCriteria(configuration == "Release")
    .IsDependentOn("pack-node-vscode")
    .Does(() => UploadVSCode("ovsx", ovsxToken));

RunTarget(target);
