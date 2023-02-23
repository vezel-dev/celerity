namespace Vezel.Celerity.Language.Syntax.Tree;

public abstract class SyntaxItem
{
    public SyntaxItem? Parent { get; private set; }

    public abstract bool HasChildren { get; }

    internal void SetParent(SyntaxItem parent)
    {
        Parent = parent;
    }

    private protected SyntaxItem()
    {
    }

    public DocumentSyntax GetDocument()
    {
        // DocumentNode is the only syntax item with a null Parent.
        return Parent != null ? Parent.GetDocument() : Unsafe.As<DocumentSyntax>(this);
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
