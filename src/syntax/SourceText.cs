namespace Vezel.Celerity.Syntax;

public abstract class SourceText : IReadOnlyList<char>
{
    public string Path { get; }

    public abstract int Count { get; }

    public abstract char this[int index] { get; }

    protected SourceText(string path)
    {
        Check.NullOrEmpty(path);

        Path = path;
    }

    public IEnumerable<SourceTextLine> EnumerateLines()
    {
        var reader = new SyntaxInputReader<char>(this);
        var builder = new StringBuilder();
        var line = 1;

        while (reader.Count != 0)
        {
            var ch = reader.Read();

            _ = builder.Append(ch);

            if (ch is '\r' or '\n')
            {
                if ((ch, reader.Peek1()) == ('\r', (true, '\n')))
                    _ = builder.Append(reader.Read());

                yield return new(new(Path, line++, 1), builder.ToString());

                _ = builder.Clear();
            }
        }
    }

    public abstract IEnumerator<char> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
