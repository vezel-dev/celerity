namespace Vezel.Celerity.Syntax;

public abstract class SourceText
{
    public static Encoding Encoding { get; } = new UTF8Encoding(false, true);

    public string FullPath { get; }

    protected SourceText(string fullPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(fullPath);

        FullPath = fullPath;
    }

    public abstract IEnumerable<Rune> EnumerateRunes();
}
