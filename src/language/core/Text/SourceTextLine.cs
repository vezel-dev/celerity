namespace Vezel.Celerity.Language.Text;

public readonly struct SourceTextLine :
    IEquatable<SourceTextLine>, IEqualityOperators<SourceTextLine, SourceTextLine, bool>
{
    public SourceLocation Location { get; }

    public string Text { get; }

    internal SourceTextLine(SourceLocation location, string text)
    {
        Location = location;
        Text = text;
    }

    public static bool operator ==(SourceTextLine left, SourceTextLine right) => left.Equals(right);

    public static bool operator !=(SourceTextLine left, SourceTextLine right) => !left.Equals(right);

    public bool Equals(SourceTextLine other)
    {
        return (Location, Text) == (other.Location, other.Text);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is SourceTextLine other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Location, Text);
    }

    public override string ToString()
    {
        return Text;
    }
}
