namespace Vezel.Celerity.Tests.Adapter;

[ExtensionUri(ExecutorUri)]
[SuppressMessage("", "CA1001")]
public sealed class CelerityTestExecutor : ITestExecutor
{
    internal const string ExecutorUri = "executor://" + nameof(CelerityTestExecutor);

    private readonly CancellationTokenSource _cts = new();

    public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(runContext);
        ArgumentNullException.ThrowIfNull(frameworkHandle);

        RunTests(tests.Select(t => (CelerityTestLoader.Tests[t.FullyQualifiedName], t)), frameworkHandle);
    }

    public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
    {
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(runContext);
        ArgumentNullException.ThrowIfNull(frameworkHandle);

        RunTests(CelerityTestLoader.Tests.Values.Select(x => (x, x.Convert())), frameworkHandle);
    }

    private void RunTests(
        IEnumerable<(CelerityTestCase AdapterCase, TestCase ConvertedCase)> tests, IFrameworkHandle frameworkHandle)
    {
        try
        {
            foreach (var (tc, vstc) in tests)
            {
                _cts.Token.ThrowIfCancellationRequested();

                frameworkHandle.RecordStart(vstc);

                var start = DateTimeOffset.Now;
                var result = tc.Run(_cts.Token);
                var end = DateTimeOffset.Now;

                frameworkHandle.RecordEnd(vstc, result.Outcome);

                var vstr = new TestResult(vstc)
                {
                    Outcome = result.Outcome,
                    ErrorMessage = result.Error,
                    Duration = end - start,
                    StartTime = start,
                    EndTime = end,
                    ComputerName = Environment.MachineName,
                };

                vstr.Messages.Add(new(TestResultMessage.StandardOutCategory, result.StandardOut));
                vstr.Messages.Add(new(TestResultMessage.StandardErrorCategory, result.StandardError));

                frameworkHandle.RecordResult(vstr);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Cancel()
    {
        _cts.Cancel();
    }
}
