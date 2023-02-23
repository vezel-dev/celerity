using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax.Tree;

public sealed class SyntaxTrivia : SyntaxItem
{
    public new SyntaxToken Parent => Unsafe.As<SyntaxToken>(base.Parent!);

    public override bool HasChildren => false;

    public SourceLocation Location { get; }

    public SyntaxTriviaKind Kind { get; }

    public string Text { get; }

    internal SyntaxTrivia(SourceLocation location, SyntaxTriviaKind kind, string text)
    {
        Location = location;
        Kind = kind;
        Text = text;
    }

    public new IEnumerable<SyntaxTrivia> Siblings()
    {
        return base.Siblings().UnsafeCast<SyntaxTrivia>();
    }

    public new IEnumerable<SyntaxTrivia> SiblingsAndSelf()
    {
        return base.SiblingsAndSelf().UnsafeCast<SyntaxTrivia>();
    }

    public override IEnumerable<SyntaxItem> Children()
    {
        return Array.Empty<SyntaxItem>();
    }

    public override IEnumerable<SyntaxItem> Descendants()
    {
        return Array.Empty<SyntaxItem>();
    }

    public override string ToString()
    {
        return Text;
    }
}
