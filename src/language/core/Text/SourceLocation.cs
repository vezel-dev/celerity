namespace Vezel.Celerity.Language.Text;

public readonly struct SourceLocation :
    IEquatable<SourceLocation>, IEqualityOperators<SourceLocation, SourceLocation, bool>
{
    public static SourceLocation Missing { get; }

    public string? Path { get; }

    public SourceTextSpan Span { get; }

    public SourceTextLinePosition Start { get; }

    public SourceTextLinePosition End { get; }

    [MemberNotNullWhen(false, "Path")]
    public bool IsMissing => Path == null;

    internal SourceLocation(string path, SourceTextSpan span, SourceTextLinePosition start, SourceTextLinePosition end)
    {
        Path = path;
        Span = span;
        Start = start;
        End = end;
    }

    public static bool operator ==(SourceLocation left, SourceLocation right) => left.Equals(right);

    public static bool operator !=(SourceLocation left, SourceLocation right) => !left.Equals(right);

    public bool Equals(SourceLocation other)
    {
        return (Path, Span, Start, End) == (other.Path, other.Span, other.Start, other.End);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is SourceLocation other && Equals(other);
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
