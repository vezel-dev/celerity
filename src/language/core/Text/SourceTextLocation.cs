namespace Vezel.Celerity.Language.Text;

public sealed class SourceTextLocation :
    IEquatable<SourceTextLocation>, IEqualityOperators<SourceTextLocation, SourceTextLocation, bool>
{
    public string Path { get; }

    public SourceTextSpan Span { get; }

    public SourceTextLinePosition Start { get; }

    public SourceTextLinePosition End { get; }

    internal SourceTextLocation(string path, SourceTextSpan span, SourceTextLinePosition start, SourceTextLinePosition end)
    {
        Path = path;
        Span = span;
        Start = start;
        End = end;
    }

    public static bool operator ==(SourceTextLocation? left, SourceTextLocation? right) =>
        EqualityComparer<SourceTextLocation>.Default.Equals(left, right);

    public static bool operator !=(SourceTextLocation? left, SourceTextLocation? right) => !(left == right);

    public bool Equals(SourceTextLocation? other)
    {
        return (Path, Span, Start, End) == (other?.Path, other?.Span, other?.Start, other?.End);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return Equals(obj as SourceTextLocation);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Path, Span, Start, End);
    }

    public override string ToString()
    {
        return $"{Path} ({Start})-({End})";
    }
}
