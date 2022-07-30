namespace Vezel.Celerity.Tests.Adapter;

[DefaultExecutorUri(CelerityTestExecutor.ExecutorUri)]
[FileExtension(".dll")]
public sealed class CelerityTestDiscoverer : ITestDiscoverer
{
    public void DiscoverTests(
        IEnumerable<string> sources,
        IDiscoveryContext discoveryContext,
        IMessageLogger logger,
        ITestCaseDiscoverySink discoverySink)
    {
        foreach (var test in CelerityTestLoader.Tests.Values)
            discoverySink.SendTestCase(test.Convert());
    }
}
