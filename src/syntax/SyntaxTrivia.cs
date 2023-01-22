namespace Vezel.Celerity.Syntax;

public sealed class SyntaxTrivia : SyntaxItem
{
    public new SyntaxToken Parent => Unsafe.As<SyntaxToken>(base.Parent!);

    public SourceLocation Location { get; }

    public SyntaxTriviaKind Kind { get; }

    public string Text { get; }

    internal SyntaxTrivia(SourceLocation location, SyntaxTriviaKind kind, string text)
    {
        Location = location;
        Kind = kind;
        Text = text;
    }

    public override string ToString()
    {
        return Text;
    }
}
