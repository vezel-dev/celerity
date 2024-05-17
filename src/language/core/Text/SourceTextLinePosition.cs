// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Text;

public readonly struct SourceTextLinePosition :
    IEquatable<SourceTextLinePosition>,
    IEqualityOperators<SourceTextLinePosition, SourceTextLinePosition, bool>,
    IComparable<SourceTextLinePosition>,
    IComparisonOperators<SourceTextLinePosition, SourceTextLinePosition, bool>
{
    public int Line { get; }

    public int Character { get; }

    internal SourceTextLinePosition(int line, int character)
    {
        Line = line;
        Character = character;
    }

    public static bool operator ==(SourceTextLinePosition left, SourceTextLinePosition right) => left.Equals(right);

    public static bool operator !=(SourceTextLinePosition left, SourceTextLinePosition right) => !left.Equals(right);

    public static bool operator <(SourceTextLinePosition left, SourceTextLinePosition right) =>
        left.CompareTo(right) < 0;

    public static bool operator <=(SourceTextLinePosition left, SourceTextLinePosition right) =>
        left.CompareTo(right) <= 0;

    public static bool operator >(SourceTextLinePosition left, SourceTextLinePosition right) =>
        left.CompareTo(right) > 0;

    public static bool operator >=(SourceTextLinePosition left, SourceTextLinePosition right) =>
        left.CompareTo(right) >= 0;

    public int CompareTo(SourceTextLinePosition other)
    {
        var result = Line.CompareTo(other.Line);

        return result != 0 ? result : Character.CompareTo(other.Character);
    }

    public bool Equals(SourceTextLinePosition other)
    {
        return (Line, Character) == (other.Line, other.Character);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is SourceTextLinePosition other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Line, Character);
    }

    public override string ToString()
    {
        return $"{Line},{Character}";
    }
}
