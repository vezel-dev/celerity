namespace Vezel.Celerity.VisualStudio;

[SuppressMessage("", "CA1812")]
[VisualStudioContribution]
internal sealed class CelerityExtension : Extension
{
    public override ExtensionConfiguration ExtensionConfiguration { get; } =
        new()
        {
            Metadata = new(
                "celerity",
                Version.Parse(ThisAssembly.AssemblyFileVersion),
                "Vezel",
                "Celerity",
                "Celerity programming language support for Visual Studio."),
        };
}
