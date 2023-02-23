using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax.Tree;

public sealed class SyntaxToken : SyntaxItem
{
    public new SyntaxNode Parent => Unsafe.As<SyntaxNode>(base.Parent!);

    public override bool HasChildren => !LeadingTrivia.IsEmpty || !TrailingTrivia.IsEmpty;

    public SourceLocation Location { get; }

    public SyntaxTokenKind Kind { get; }

    public bool IsMissing => Kind == SyntaxTokenKind.Missing;

    public bool IsEndOfInput => Kind == SyntaxTokenKind.EndOfInput;

    public bool IsUnrecognized => Kind == SyntaxTokenKind.Unrecognized;

    public string Text { get; }

    public object? Value { get; }

    public ImmutableArray<SyntaxTrivia> LeadingTrivia { get; }

    public ImmutableArray<SyntaxTrivia> TrailingTrivia { get; }

    internal SyntaxToken(string path)
        : this(
            new(path),
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

    public new IEnumerable<SyntaxNode> Ancestors()
    {
        return base.Ancestors().UnsafeCast<SyntaxNode>();
    }

    public override IEnumerable<SyntaxTrivia> Children()
    {
        foreach (var leading in LeadingTrivia)
            yield return leading;

        foreach (var trailing in TrailingTrivia)
            yield return trailing;
    }

    public override IEnumerable<SyntaxTrivia> Descendants()
    {
        return Children();
    }

    public override string ToString()
    {
        return Text;
    }
}
