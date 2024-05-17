// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Text;

public readonly struct SourceTextSpan :
    IEquatable<SourceTextSpan>,
    IEqualityOperators<SourceTextSpan, SourceTextSpan, bool>,
    IComparable<SourceTextSpan>,
    IComparisonOperators<SourceTextSpan, SourceTextSpan, bool>
{
    public static SourceTextSpan Empty { get; } = new(start: 0, length: 0);

    public int Start { get; }

    public int End => Start + Length;

    public int Length { get; }

    public bool IsEmpty => Length == 0;

    internal SourceTextSpan(int start, int length)
    {
        Start = start;
        Length = length;
    }

    public static bool operator ==(SourceTextSpan left, SourceTextSpan right) => left.Equals(right);

    public static bool operator !=(SourceTextSpan left, SourceTextSpan right) => !left.Equals(right);

    public static bool operator <(SourceTextSpan left, SourceTextSpan right) => left.CompareTo(right) < 0;

    public static bool operator <=(SourceTextSpan left, SourceTextSpan right) => left.CompareTo(right) <= 0;

    public static bool operator >(SourceTextSpan left, SourceTextSpan right) => left.CompareTo(right) > 0;

    public static bool operator >=(SourceTextSpan left, SourceTextSpan right) => left.CompareTo(right) >= 0;

    public static SourceTextSpan Union(SourceTextSpan first, SourceTextSpan second)
    {
        var start = int.Min(first.Start, second.Start);

        return new(start, int.Max(first.End, second.End) - start);
    }

    public bool Contains(int position)
    {
        Check.Range(position >= 0, position);

        return position >= Start && position < End;
    }

    public bool Contains(SourceTextSpan span)
    {
        return span.Start >= Start && span.End <= End;
    }

    public bool Overlaps(SourceTextSpan span)
    {
        return int.Max(Start, span.Start) < int.Min(End, span.End);
    }

    public int CompareTo(SourceTextSpan other)
    {
        var diff = Start - other.Start;

        return diff != 0 ? diff : Length - other.Length;
    }

    public bool Equals(SourceTextSpan other)
    {
        return (Start, Length) == (other.Start, other.Length);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is SourceTextSpan other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, Length);
    }

    public override string ToString()
    {
        return $"{Start}..{End}";
    }
}
