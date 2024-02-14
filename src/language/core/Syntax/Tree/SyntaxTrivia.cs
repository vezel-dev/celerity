using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax.Tree;

public sealed class SyntaxTrivia : SyntaxTerminal
{
    public new SyntaxToken Parent => Unsafe.As<SyntaxToken>(base.Parent!);

    public override SourceTextSpan Span => FullSpan;

    public override SourceTextSpan FullSpan => new(_position, Text.Length);

    public override bool HasChildren => false;

    public SyntaxTriviaKind Kind { get; }

    public bool IsSkippedToken => Kind == SyntaxTriviaKind.SkippedToken;

    private readonly int _position;

    internal SyntaxTrivia(int position, SyntaxTriviaKind kind, string text)
        : base(text)
    {
        _position = position;
        Kind = kind;
    }

    public new IEnumerable<SyntaxTrivia> Siblings()
    {
        return base.Siblings().UnsafeCast<SyntaxTrivia>();
    }

    public new IEnumerable<SyntaxTrivia> SiblingsAndSelf()
    {
        return base.SiblingsAndSelf().UnsafeCast<SyntaxTrivia>();
    }

    public override IEnumerable<SyntaxTerminal> Children()
    {
        return [];
    }

    public new IEnumerable<SyntaxTrivia> ChildrenAndSelf()
    {
        return base.ChildrenAndSelf().UnsafeCast<SyntaxTrivia>();
    }

    public override IEnumerable<SyntaxTerminal> Descendants()
    {
        return [];
    }

    public new IEnumerable<SyntaxTrivia> DescendantsAndSelf()
    {
        return base.DescendantsAndSelf().UnsafeCast<SyntaxTrivia>();
    }

    public override string ToFullString()
    {
        return ToString();
    }

    internal override void ToString(StringBuilder builder, bool leading, bool trailing)
    {
        _ = builder.Append(Text);
    }
}
