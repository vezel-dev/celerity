using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax.Tree;

public abstract class SyntaxItem
{
    public SyntaxTree Tree => _parent is SyntaxTree tree ? tree : Unsafe.As<SyntaxItem>(_parent).Tree;

    // Checking for SyntaxTree is faster since it is sealed, while SyntaxItem is not.
    public SyntaxItem? Parent => _parent is SyntaxTree ? null : Unsafe.As<SyntaxItem>(_parent);

    public abstract SourceTextSpan Span { get; }

    public abstract SourceTextSpan FullSpan { get; }

    public abstract bool HasChildren { get; }

    private object _parent = null!;

    private protected SyntaxItem()
    {
    }

    internal void SetParent(object parent)
    {
        _parent = parent;
    }

    public IEnumerable<SyntaxItem> Ancestors()
    {
        return Parent?.AncestorsAndSelf() ?? Array.Empty<SyntaxItem>();
    }

    public IEnumerable<SyntaxItem> AncestorsAndSelf()
    {
        var node = this;

        do
        {
            yield return node;

            node = node.Parent;
        }
        while (node != null);
    }

    public IEnumerable<SyntaxItem> Siblings()
    {
        return SiblingsAndSelf().Where(item => item != this);
    }

    public IEnumerable<SyntaxItem> SiblingsAndSelf()
    {
        foreach (var sibling in Parent?.Children() ?? Array.Empty<SyntaxItem>())
            yield return sibling;
    }

    public abstract IEnumerable<SyntaxItem> Children();

    public IEnumerable<SyntaxItem> ChildrenAndSelf()
    {
        yield return this;

        foreach (var child in Children())
            yield return child;
    }

    public abstract IEnumerable<SyntaxItem> Descendants();

    public IEnumerable<SyntaxItem> DescendantsAndSelf()
    {
        yield return this;

        foreach (var descendant in Descendants())
            yield return descendant;
    }

    public SourceTextLocation GetLocation()
    {
        return Tree.GetText().GetLocation(Span);
    }

    public SourceTextLocation GetFullLocation()
    {
        return Tree.GetText().GetLocation(FullSpan);
    }

    public SourceText GetText()
    {
        return new StringSourceText(Tree.Path, ToString());
    }

    public SourceText GetFullText()
    {
        return new StringSourceText(Tree.Path, ToFullString());
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        ToString(sb, false, false);

        return sb.ToString();
    }

    public virtual string ToFullString()
    {
        var sb = new StringBuilder();

        ToString(sb, true, true);

        return sb.ToString();
    }

    internal abstract void ToString(StringBuilder builder, bool leading, bool trailing);
}
