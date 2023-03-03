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

    public SourceTextLocation GetLocation()
    {
        return Tree.Text.GetLocation(Span);
    }

    public SourceTextLocation GetFullLocation()
    {
        return Tree.Text.GetLocation(FullSpan);
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

    public abstract IEnumerable<SyntaxItem> Descendants();

    public override sealed string ToString()
    {
        return Tree.Text.ToString(Span);
    }

    public string ToFullString()
    {
        return Tree.Text.ToString(FullSpan);
    }
}
