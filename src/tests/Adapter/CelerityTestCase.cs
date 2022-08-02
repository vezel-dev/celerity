namespace Vezel.Celerity.Tests.Adapter;

internal sealed partial class CelerityTestCase
{
    private static readonly string _source = typeof(ThisAssembly).Assembly.Location;

    private static readonly string _executable = Path.Combine(
        Path.GetDirectoryName(_source)!,
        "..",
        "..",
        "..",
        "cli",
        "bin",
        ThisAssembly.AssemblyConfiguration,
        "celerity.exe");

    public string Name { get; }

    public string FullName => $"{ThisAssembly.RootNamespace}.{Name}";

    public FileInfo File { get; }

    private readonly string _command;

    private readonly string _arguments;

    private readonly IReadOnlyDictionary<string, string> _variables;

    private readonly bool _expectation;

    private readonly string _stdout;

    private readonly string _stderr;

    public CelerityTestCase(
        string name,
        FileInfo file,
        string command,
        string arguments,
        IReadOnlyDictionary<string, string> variables,
        bool expectation,
        string stdout,
        string stderr)
    {
        Name = name;
        File = file;
        _command = command;
        _arguments = arguments;
        _variables = variables;
        _expectation = expectation;
        _stdout = stdout;
        _stderr = stderr;
    }

    public TestCase Convert()
    {
        return new TestCase(FullName, new(CelerityTestExecutor.ExecutorUri), _source)
        {
            CodeFilePath = File.FullName,
        };
    }

    [SuppressMessage("", "CA1031")]
    public CelerityTestResult Run(CancellationToken cancellationToken)
    {
        var info = new ProcessStartInfo(_executable, $"{_command} {File.Name} {_arguments}")
        {
            WorkingDirectory = File.DirectoryName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (var (key, value) in _variables)
            info.Environment[key] = value;

        using var proc = new Process
        {
            StartInfo = info,
        };

        static void Write(StringBuilder builder, string? value)
        {
            if (value != null)
                lock (builder)
                    _ = builder.AppendLine(value);
        }

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        proc.OutputDataReceived += (_, e) => Write(stdout, e.Data);
        proc.ErrorDataReceived += (_, e) => Write(stderr, e.Data);

        _ = proc.Start();

        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        try
        {
            proc.WaitForExitAsync(cancellationToken).GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
            try
            {
                proc.Kill(true);
            }
            catch (Exception)
            {
                // Nothing we can do if this fails.
            }

            throw;
        }

        var outcome = proc.ExitCode == 0 ? TestOutcome.Passed : TestOutcome.Failed;
        var error = (outcome, _expectation) switch
        {
            (TestOutcome.Passed, false) => "Test passed but was expected to fail.",
            (TestOutcome.Failed, true) => "Test failed but was expected to pass.",
            _ => null,
        };

        static string NormalizeOutput(StringBuilder output)
        {
            // Normalize line endings, and file paths in diagnostics.
            return DiagnosticRegex().Replace(
                output.ToString().ReplaceLineEndings().Trim(),
                m => m.Groups[1].Value.Replace("\\", "/", StringComparison.Ordinal) + m.Groups[2].Value);
        }

        var stdout2 = NormalizeOutput(stdout);
        var stderr2 = NormalizeOutput(stderr);

        if (stdout2 != _stdout)
        {
            error = "Standard output text did not match the expected text.";
            outcome = TestOutcome.Failed;
        }
        else if (stderr2 != _stderr)
        {
            error = "Standard error text did not match the expected text.";
            outcome = TestOutcome.Failed;
        }

        return new(outcome, error, stdout2, stderr2);
    }

    [RegexGenerator(@"^(.*)(\(\d+,\d+\): .*: .*)$", RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex DiagnosticRegex();
}
