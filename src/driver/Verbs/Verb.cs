namespace Vezel.Celerity.Driver.Verbs;

internal abstract class Verb
{
    public abstract Task<int> RunAsync();
}
