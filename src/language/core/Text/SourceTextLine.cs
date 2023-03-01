namespace Vezel.Celerity.Language.Text;

public readonly struct SourceTextLine :
    IEquatable<SourceTextLine>, IEqualityOperators<SourceTextLine, SourceTextLine, bool>
{
    public SourceText Text { get; }

    public SourceTextSpan Span { get; }

    public int Line { get; }

    internal SourceTextLine(SourceText text, SourceTextSpan span, int line)
    {
        Text = text;
        Span = span;
        Line = line;
    }

    public static bool operator ==(SourceTextLine left, SourceTextLine right) => left.Equals(right);

    public static bool operator !=(SourceTextLine left, SourceTextLine right) => !left.Equals(right);

    public bool Equals(SourceTextLine other)
    {
        return (Text, Span, Line) == (other.Text, other.Span, other.Line);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is SourceTextLine other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Text, Span, Line);
    }

    public override string ToString()
    {
        return Text.ToString(Span);
    }
}
