#addin nuget:?package=Cake.DoInDirectory&version=6.0.0
#addin nuget:?package=Cake.GitVersioning&version=3.6.133
#addin nuget:?package=Cake.Npm&version=4.0.0
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
        // TODO: https://github.com/dotnet/msbuild/issues/6756
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
        ArgumentCustomization = args => args.Append("-ds:false"),
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

void RunCommand(
    string name,
    string[] tools,
    Func<ProcessArgumentBuilder, ProcessArgumentBuilder> appender,
    Func<string, string, bool> checker)
{
    var code = StartProcess(
        Context.Tools.Resolve(tools) ?? throw new CakeException($"{name}: Could not locate executable."),
        new()
        {
            Arguments = appender(new()),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectedStandardOutputHandler = text =>
            {
                Console.Out.WriteLine(text);

                return text;
            },
            RedirectedStandardErrorHandler = text =>
            {
                Console.Error.WriteLine(text);

                return text;
            },
        },
        out var stdOut,
        out var stdErr);

    if (code != 0 && checker(string.Join(null, stdOut), string.Join(null, stdErr)))
        throw new CakeException(code, $"{name}: Process returned an error (exit code {code}).");
}

void RunDotNet(Func<ProcessArgumentBuilder, ProcessArgumentBuilder> appender, Func<string, string, bool> checker)
{
    RunCommand(".NET CLI", new[] { "dotnet", "dotnet.exe" }, appender, checker);
}

void RunVSCode(Func<ProcessArgumentBuilder, ProcessArgumentBuilder> appender)
{
    RunCommand("VS Code", new[] { "code.cmd", "code" }, appender, (_, _) => true);
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

Task("default-editor")
    .IsDependentOn("build")
    .IsDependentOn("pack");

Task("restore-core")
    .Does(() =>
        DotNetRestore(
            celerityProj.FullPath,
            new()
            {
                MSBuildSettings = ConfigureMSBuild("restore"),
            }));

Task("restore-vscode")
    .Does(() => NpmInstall(srcExtensionsVscode));

Task("restore-doc")
    .Does(() => NpmInstall(doc));

Task("restore")
    .IsDependentOn("restore-core")
    .IsDependentOn("restore-vscode")
    .IsDependentOn("restore-doc");

Task("build-core")
    .IsDependentOn("restore-core")
    .Does(() =>
        DotNetBuild(
            celerityProj.FullPath,
            new()
            {
                MSBuildSettings = ConfigureMSBuild("build"),
                Configuration = configuration,
                NoRestore = true,
            }));

Task("build-trimming")
    .IsDependentOn("build-core")
    .Does(() =>
        DotNetPublish(
            trimmingCsproj.FullPath,
            new()
            {
                MSBuildSettings = ConfigureMSBuild("publish"),
                Configuration = configuration,
                NoBuild = true,
            }));

Task("build-stdlib")
    .IsDependentOn("build-core")
    .Does(() =>
        DoInDirectory(
            srcLanguageLibrary,
            () =>
            {
                foreach (var cmd in new[] { "check", "format" })
                    DotNetRun(driverCsproj.FullPath, args => args.Append(cmd));
            }));

Task("build-vscode")
    .IsDependentOn("restore-vscode")
    .Does(() => DoInDirectory(srcExtensionsVscode, () => NpmRunScript("build")));

Task("build-doc")
    .IsDependentOn("restore-doc")
    .Does(() => DoInDirectory(doc, () => NpmRunScript("build")));

Task("build")
    .IsDependentOn("build-stdlib")
    .IsDependentOn("build-trimming")
    .IsDependentOn("build-vscode")
    .IsDependentOn("build-doc");

Task("pack-core")
    .IsDependentOn("build-core")
    .Does(() =>
        DotNetPack(
            celerityProj.FullPath,
            new()
            {
                MSBuildSettings = ConfigureMSBuild("pack"),
                Configuration = configuration,
                NoBuild = true,
            }));

Task("pack-vscode")
    .IsDependentOn("build-vscode")
    .Does(() => DoInDirectory(srcExtensionsVscode, () => NpmRunScript("pack")));

Task("pack")
    .IsDependentOn("pack-core")
    .IsDependentOn("pack-vscode");

Task("test-core")
    .IsDependentOn("build-core")
    .Does(() =>
        DotNetTest(
            testsCsproj.FullPath,
            new()
            {
                MSBuildSettings = ConfigureMSBuild("test"),
                Configuration = configuration,
                NoBuild = true,
            }));

Task("test-benchmarks")
    .IsDependentOn("build-core")
    .Does(() => DotNetRun(benchmarksCsproj, args => args.Append("-t")));

Task("test-stdlib")
    .IsDependentOn("build-stdlib")
    .Does(() => DotNetRun(driverCsproj.FullPath, args => args.Append("test")));

Task("test")
    .IsDependentOn("test-core")
    .IsDependentOn("test-benchmarks")
    .IsDependentOn("test-stdlib");

Task("benchmark-core")
    .WithCriteria(configuration == "Release")
    .IsDependentOn("build-core")
    .Does(() =>
        DotNetRun(benchmarksCsproj, args => filter != null ? args.AppendSwitchQuoted("-f", filter) : args));

Task("benchmark")
    .IsDependentOn("benchmark-core");

Task("install-core")
    .IsDependentOn("pack-core")
    .Does(() =>
        RunDotNet(
            args =>
                args
                    .Append("tool")
                    .Append("update")
                    .Append("celerity")
                    .Append("--prerelease")
                    .Append("-g"),
            (_, _) => true));

Task("install-vscode")
    .IsDependentOn("pack-vscode")
    .Does(() =>
    {
        var version = GitVersioningGetVersion(root.FullPath);

        RunVSCode(
            args => args.AppendSwitchQuoted(
                "--install-extension",
                outPkgVscode.CombineWithFilePath($"celerity-{version.NpmPackageVersion}.vsix").FullPath));
    });

Task("install")
    .IsDependentOn("install-core")
    .IsDependentOn("install-vscode");

Task("uninstall-core")
    .Does(() =>
        RunDotNet(
            args =>
                args
                    .Append("tool")
                    .Append("uninstall")
                    .Append("celerity")
                    .Append("-g"),
            (_, stdErr) => !stdErr.StartsWith("A tool with the package Id 'celerity' could not be found.")));

Task("uninstall-vscode")
    .Does(() => RunVSCode(args => args.AppendSwitchQuoted("--uninstall-extension", "vezel.celerity")));

Task("uninstall")
    .IsDependentOn("uninstall-core")
    .IsDependentOn("uninstall-vscode");

Task("upload-core-github")
    .WithCriteria(BuildSystem.GitHubActions.Environment.Workflow.Ref == "refs/heads/master")
    .WithCriteria(configuration == "Debug")
    .IsDependentOn("pack-core")
    .Does(() =>
        DotNetTool(
            null,
            "gpr push",
            new ProcessArgumentBuilder()
                .AppendQuoted(githubGlob)
                .AppendSwitchQuotedSecret("-k", githubToken)));

Task("upload-core-nuget")
    .WithCriteria(BuildSystem.GitHubActions.Environment.Workflow.Ref.StartsWith("refs/tags/v"))
    .WithCriteria(configuration == "Release")
    .IsDependentOn("pack-core")
    .Does(() =>
    {
        foreach (var pkg in GetFiles(nugetGlob))
            DotNetNuGetPush(
                pkg,
                new()
                {
                    Source = "https://api.nuget.org/v3/index.json",
                    ApiKey = nugetToken,
                    SkipDuplicate = true,
                });
    });

Task("upload-vscode-vsce")
    .WithCriteria(BuildSystem.GitHubActions.Environment.Workflow.Ref.StartsWith("refs/tags/v"))
    .WithCriteria(configuration == "Release")
    .IsDependentOn("pack-vscode")
    .Does(() => UploadVSCode("vsce", vsceToken));

Task("upload-vscode-ovsx")
    .WithCriteria(BuildSystem.GitHubActions.Environment.Workflow.Ref.StartsWith("refs/tags/v"))
    .WithCriteria(configuration == "Release")
    .IsDependentOn("pack-vscode")
    .Does(() => UploadVSCode("ovsx", ovsxToken));

RunTarget(target);
