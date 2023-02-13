namespace Vezel.Celerity.Syntax.Text;

public readonly struct SourceLocation :
    IEquatable<SourceLocation>, IEqualityOperators<SourceLocation, SourceLocation, bool>
{
    public string Path { get; }

    public int Line { get; }

    public int Character { get; }

    public bool IsMissing => (Line, Character) == (0, 0);

    internal SourceLocation(string path)
        : this(path, 0, 0)
    {
    }

    internal SourceLocation(string path, int line, int character)
    {
        Path = path;
        Line = line;
        Character = character;
    }

    public static bool operator ==(SourceLocation left, SourceLocation right) => left.Equals(right);

    public static bool operator !=(SourceLocation left, SourceLocation right) => !left.Equals(right);

    public bool Equals(SourceLocation other)
    {
        return (Path, Line, Character) == (other.Path, other.Line, other.Character);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is SourceLocation other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Path, Line, Character);
    }

    public override string ToString()
    {
        return $"{Path}({Line},{Character})";
    }
}
