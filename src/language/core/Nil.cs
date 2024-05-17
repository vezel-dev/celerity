// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language;

public sealed class Nil :
    IEquatable<Nil>, IEqualityOperators<Nil, Nil, bool>, IComparable<Nil>, IComparisonOperators<Nil, Nil, bool>
{
    public static Nil Value { get; } = new();

    private Nil()
    {
    }

    public static bool operator ==(Nil? left, Nil? right) => EqualityComparer<Nil>.Default.Equals(left, right);

    public static bool operator !=(Nil? left, Nil? right) => !(left == right);

    public static bool operator <(Nil left, Nil right) => Comparer<Nil>.Default.Compare(left, right) < 0;

    public static bool operator <=(Nil left, Nil right) => Comparer<Nil>.Default.Compare(left, right) <= 0;

    public static bool operator >(Nil left, Nil right) => right < left;

    public static bool operator >=(Nil left, Nil right) => right <= left;

    public bool Equals(Nil? other)
    {
        return other != null;
    }

    public int CompareTo(Nil? other)
    {
        return other == null ? 1 : 0;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Nil);
    }

    public override int GetHashCode()
    {
        // Appease the compiler (CS0659, CS0661).
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return "nil";
    }
}
