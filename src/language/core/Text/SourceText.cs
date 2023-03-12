namespace Vezel.Celerity.Language.Text;

public abstract class SourceText : IReadOnlyList<char>
{
    public string Path { get; }

    public abstract int Count { get; }

    public abstract char this[int index] { get; }

    public SourceTextLineList Lines => _lines.Value;

    private readonly Lazy<SourceTextLineList> _lines;

    protected SourceText(string path)
    {
        Check.NullOrEmpty(path);

        Path = path;
        _lines = new(() =>
        {
            var reader = new ListReader<char>(this);
            var builder = ImmutableArray.CreateBuilder<SourceTextLine>();

            var position = 0;
            var line = 0;
            var charsInLine = 0;

            char Read()
            {
                var ch = reader.Read();

                position++;
                charsInLine++;

                return ch;
            }

            while (!reader.IsEmpty)
            {
                var ch1 = Read();

                if (TextFacts.IsNewLine(ch1))
                {
                    _ = reader.TryPeek(0, out var ch2);

                    if ((ch1, ch2) == ('\r', '\n'))
                        _ = Read();

                    builder.Add(new(this, new(position - charsInLine, charsInLine), line++));

                    charsInLine = 0;
                }
            }

            // Edge cases: The file is empty, or the last line lacks a line ending.
            if (line == 0 || charsInLine != 0)
                builder.Add(new(this, new(0, charsInLine), line));

            return new(this, builder.DrainToImmutable());
        });
    }

    public SourceTextLocation GetLocation(SourceTextSpan span)
    {
        var lines = Lines;

        return new(Path, span, lines.GetLinePosition(span.Start), lines.GetLinePosition(span.End));
    }

    public abstract IEnumerator<char> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override sealed string ToString()
    {
        return ToString(new(0, Count));
    }

    public abstract string ToString(SourceTextSpan span);
}
