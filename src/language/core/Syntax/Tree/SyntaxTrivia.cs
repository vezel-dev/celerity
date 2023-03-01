using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax.Tree;

public sealed class SyntaxTrivia : SyntaxItem
{
    public new SyntaxToken Parent => Unsafe.As<SyntaxToken>(base.Parent!);

    public override SourceTextSpan Span => FullSpan;

    public override SourceTextSpan FullSpan => new(_position, Text.Length);

    public override bool HasChildren => false;

    public SyntaxTriviaKind Kind { get; }

    public string Text { get; }

    private readonly int _position;

    internal SyntaxTrivia(int position, SyntaxTriviaKind kind, string text)
    {
        _position = position;
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
