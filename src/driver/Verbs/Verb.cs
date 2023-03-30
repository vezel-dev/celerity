namespace Vezel.Celerity.Driver.Verbs;

internal abstract class Verb
{
    public abstract ValueTask<int> RunAsync(CancellationToken cancellationToken);
}
