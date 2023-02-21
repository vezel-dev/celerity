using Vezel.Celerity.Diagnostics;

namespace Vezel.Celerity.Text;

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
        var reader = new ListReader<char>(this);
        var builder = new StringBuilder();
        var line = 1;

        while (!reader.IsEmpty)
        {
            var ch1 = reader.Read();

            _ = builder.Append(ch1);

            if (ch1 is '\r' or '\n')
            {
                _ = reader.TryPeek(0, out var ch2);

                if ((ch1, ch2) == ('\r', '\n'))
                    _ = builder.Append(reader.Read());

                yield return new(new(Path, line++, 1), builder.ToString());

                _ = builder.Clear();
            }
        }

        // Edge cases: The file is empty, or the last line lacks a line ending.
        if (line == 1 || builder.Length != 0)
            yield return new(new(Path, line, 1), builder.ToString());
    }

    public abstract IEnumerator<char> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
