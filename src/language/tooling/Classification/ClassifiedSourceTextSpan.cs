// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Tooling.Classification;

public readonly struct ClassifiedSourceTextSpan :
    IEquatable<ClassifiedSourceTextSpan>,
    IEqualityOperators<ClassifiedSourceTextSpan, ClassifiedSourceTextSpan, bool>
{
    public SourceTextSpan Span { get; }

    public SyntaxClassification Classification { get; }

    public SyntaxClassificationModifiers Modifiers { get; }

    internal ClassifiedSourceTextSpan(
        SourceTextSpan span, SyntaxClassification classification, SyntaxClassificationModifiers modifiers)
    {
        Span = span;
        Classification = classification;
        Modifiers = modifiers;
    }

    public static bool operator ==(ClassifiedSourceTextSpan left, ClassifiedSourceTextSpan right) =>
        left.Equals(right);

    public static bool operator !=(ClassifiedSourceTextSpan left, ClassifiedSourceTextSpan right) =>
        !left.Equals(right);

    public bool Equals(ClassifiedSourceTextSpan other)
    {
        return (Span, Classification, Modifiers) == (other.Span, other.Classification, other.Modifiers);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is ClassifiedSourceTextSpan other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Span, Classification, Modifiers);
    }

    public override string ToString()
    {
        return $"{Span}: {Classification} ({Modifiers})";
    }
}
