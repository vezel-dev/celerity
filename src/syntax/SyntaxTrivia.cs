namespace Vezel.Celerity.Syntax;

public sealed class SyntaxTrivia : SyntaxItem
{
    public override SyntaxToken Parent => _parent;

    public SourceLocation Location { get; }

    public SyntaxTriviaKind Kind { get; }

    public string Text { get; }

    private SyntaxToken _parent = null!;

    internal SyntaxTrivia(SourceLocation location, SyntaxTriviaKind kind, string text)
    {
        Location = location;
        Kind = kind;
        Text = text;
    }

    internal void SetParent(SyntaxToken parent)
    {
        _parent = parent;
    }

    public override string ToString()
    {
        return Text;
    }
}
