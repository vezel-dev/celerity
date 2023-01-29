namespace Vezel.Celerity.Syntax;

public sealed class SyntaxToken : SyntaxItem
{
    public new SyntaxNode Parent => Unsafe.As<SyntaxNode>(base.Parent!);

    public SourceLocation Location { get; }

    public SyntaxTokenKind Kind { get; }

    public bool IsMissing => Kind == SyntaxTokenKind.Missing;

    public bool IsEndOfInput => Kind == SyntaxTokenKind.EndOfInput;

    public bool IsUnrecognized => Kind == SyntaxTokenKind.Unrecognized;

    public string Text { get; }

    public object? Value { get; }

    public bool HasValue => Value != null;

    public ImmutableArray<SyntaxTrivia> LeadingTrivia { get; }

    public bool HasLeadingTrivia => !LeadingTrivia.IsEmpty;

    public ImmutableArray<SyntaxTrivia> TrailingTrivia { get; }

    public bool HasTrailingTrivia => !TrailingTrivia.IsEmpty;

    internal SyntaxToken(string fullPath)
        : this(
            new(fullPath),
            SyntaxTokenKind.Missing,
            "<error>",
            null,
            ImmutableArray<SyntaxTrivia>.Empty,
            ImmutableArray<SyntaxTrivia>.Empty)
    {
    }

    internal SyntaxToken(
        SourceLocation location,
        SyntaxTokenKind kind,
        string text,
        object? value,
        ImmutableArray<SyntaxTrivia> leading,
        ImmutableArray<SyntaxTrivia> trailing)
    {
        Location = location;
        Kind = kind;
        Text = text;
        Value = value;
        LeadingTrivia = leading;
        TrailingTrivia = trailing;

        foreach (var trivia in leading)
            trivia.SetParent(this);

        foreach (var trivia in trailing)
            trivia.SetParent(this);
    }

    public override string ToString()
    {
        return Text;
    }
}
