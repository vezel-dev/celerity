namespace Vezel.Celerity.Syntax;

public readonly struct SourceLocation : IEquatable<SourceLocation>
{
    public string FullPath { get; }

    public int Line { get; }

    public int Column { get; }

    public bool IsMissing => (Line, Column) == (0, 0);

    internal SourceLocation(string fullPath)
        : this(fullPath, 0, 0)
    {
    }

    internal SourceLocation(string fullPath, int line, int column)
    {
        FullPath = fullPath;
        Line = line;
        Column = column;
    }

    public static bool operator ==(SourceLocation left, SourceLocation right) => left.Equals(right);

    public static bool operator !=(SourceLocation left, SourceLocation right) => !left.Equals(right);

    public bool Equals(SourceLocation other)
    {
        return (FullPath, Line, Column) == (other.FullPath, other.Line, other.Column);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is SourceLocation other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FullPath, Line, Column);
    }

    public override string ToString()
    {
        return $"{FullPath}({Line},{Column})";
    }
}
