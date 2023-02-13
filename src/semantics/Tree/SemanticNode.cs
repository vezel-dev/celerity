namespace Vezel.Celerity.Semantics.Tree;

public abstract class SemanticNode
{
    public SyntaxNode Syntax { get; }

    public SemanticNode? Parent { get; private set; }

    public abstract bool HasChildren { get; }

    internal void SetParent(SemanticNode parent)
    {
        Parent = parent;
    }

    private protected SemanticNode(SyntaxNode syntax)
    {
        Syntax = syntax;
    }

    public DocumentSemantics GetDocument()
    {
        // DocumentSemantics is the only semantic node with a null Parent.
        return Parent != null ? Parent.GetDocument() : Unsafe.As<DocumentSemantics>(this);
    }

    public IEnumerable<SemanticNode> Ancestors()
    {
        return Parent?.AncestorsAndSelf() ?? Array.Empty<SemanticNode>();
    }

    public IEnumerable<SemanticNode> AncestorsAndSelf()
    {
        var node = this;

        do
        {
            yield return node;

            node = node.Parent;
        }
        while (node != null);
    }

    public IEnumerable<SemanticNode> Siblings()
    {
        return SiblingsAndSelf().Where(node => node != this);
    }

    public IEnumerable<SemanticNode> SiblingsAndSelf()
    {
        foreach (var sibling in Parent?.Children() ?? Array.Empty<SemanticNode>())
            yield return sibling;
    }

    public abstract IEnumerable<SemanticNode> Children();

    public IEnumerable<SemanticNode> Descendants()
    {
        var work = new Queue<SemanticNode>();

        work.Enqueue(this);

        do
        {
            var current = work.Dequeue();

            if (current.HasChildren)
                foreach (var child in current.Children())
                    work.Enqueue(child);

            yield return current;
        }
        while (work.Count != 0);
    }

    internal abstract T Visit<T>(SemanticWalker<T> walker, T state);
}
