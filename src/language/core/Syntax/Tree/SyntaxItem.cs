using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax.Tree;

public abstract class SyntaxItem
{
    public SyntaxAnalysis Analysis =>
        _parent is SyntaxAnalysis analysis ? analysis : Unsafe.As<SyntaxItem>(_parent).Analysis;

    // Checking for SyntaxAnalysis is faster since it is sealed, while SyntaxItem is not.
    public SyntaxItem? Parent => _parent is SyntaxAnalysis ? null : Unsafe.As<SyntaxItem>(_parent);

    public abstract SourceTextSpan Span { get; }

    public abstract SourceTextSpan FullSpan { get; }

    public abstract bool HasChildren { get; }

    private object _parent = null!;

    internal void SetParent(object parent)
    {
        _parent = parent;
    }

    private protected SyntaxItem()
    {
    }

    public SourceLocation GetLocation()
    {
        return Analysis.Text.GetLocation(Span);
    }

    public SourceLocation GetFullLocation()
    {
        return Analysis.Text.GetLocation(FullSpan);
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
}
