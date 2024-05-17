// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Tests;

[SuppressMessage("", "CA1050")]
[SuppressMessage("", "CA1707")]
[SuppressMessage("", "IDE1006")]
[SuppressMessage("", "SA1300")]
[SuppressMessage("", "VSTHRD200")]
public sealed partial class DriverTests : CelerityTests
{
    private sealed class DirectoryBuilder
    {
        public ImmutableArray<(string Name, string Contents)> Files { get; }

        public DirectoryBuilder()
        {
            Files = [];
        }

        private DirectoryBuilder(ImmutableArray<(string Name, string Contents)> files)
        {
            Files = files;
        }

        public DirectoryBuilder AddFile(string name, string contents)
        {
            return new(Files.Add((name, contents)));
        }
    }

    [SuppressMessage("", "CA1308")]
    private static readonly string _path =
        Path.Join(
            Path.GetDirectoryName(typeof(ThisAssembly).Assembly.Location)!,
            "..",
            "..",
            "driver",
            ThisAssembly.AssemblyConfiguration.ToLowerInvariant(),
            $"celerity{(OperatingSystem.IsWindows() ? ".exe" : string.Empty)}");

    private static async Task TestAsync(
        Func<DirectoryBuilder, DirectoryBuilder> directorySetup,
        Func<ChildProcessBuilder, ChildProcessBuilder> processSetup,
        int exitCode,
        [CallerFilePath] string file = "",
        [CallerMemberName] string name = "")
    {
        var directory = Directory.CreateTempSubdirectory(name);

        try
        {
            foreach (var (fileName, fileContents) in directorySetup(new DirectoryBuilder()).Files)
            {
                if (Path.GetDirectoryName(fileName) is { Length: not 0 } dir)
                    _ = directory.CreateSubdirectory(dir);

                await File.WriteAllTextAsync(Path.Join(directory.FullName, fileName), fileContents);
            }

            var process = processSetup(
                new ChildProcessBuilder()
                    .WithFileName(_path)
                    .WithWorkingDirectory(directory.FullName)
                    .WithCreateWindow(false)
                    .WithThrowOnError(false)).Run();
            var code = await process.Completion;

            async Task VerifyOutputAsync(ChildProcessReader reader, string kind)
            {
                _ = await Verifier
                    .Verify(await reader.TextReader.ReadToEndAsync(), "txt", sourceFile: file)
                    .UseTypeName(name)
                    .UseMethodName(kind)
                    .ToTask();
            }

            var task = Task.WhenAll(
                VerifyOutputAsync(process.StandardOut, "stdout"),
                VerifyOutputAsync(process.StandardError, "stderr"));

            try
            {
                await task;
            }
            catch (Exception)
            {
                if (task.Exception is { InnerExceptions.Count: not 1 } ex)
                    ExceptionDispatchInfo.Capture(ex).Throw();

                throw;
            }

            // Check the exit code last since the output is almost always essential in diagnosing failures.
            code.ShouldBe(exitCode);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}
