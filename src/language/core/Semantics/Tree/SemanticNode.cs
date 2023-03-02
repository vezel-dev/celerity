using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Semantics.Tree;

public abstract class SemanticNode
{
    public SyntaxNode Syntax { get; }

    public SemanticAnalysis Analysis =>
        _parent is SemanticAnalysis analysis ? analysis : Unsafe.As<SemanticNode>(_parent).Analysis;

    // Checking for SemanticAnalysis is faster since it is sealed, while SemanticNode is not.
    public SemanticNode? Parent => _parent is SemanticAnalysis ? null : Unsafe.As<SemanticNode>(_parent);

    public abstract bool HasChildren { get; }

    private object _parent = null!;

    internal void SetParent(object parent)
    {
        _parent = parent;
    }

    private protected SemanticNode(SyntaxNode syntax)
    {
        Syntax = syntax;
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
        var work = new Stack<SemanticNode>();

        work.Push(this);

        do
        {
            var current = work.Pop();

            if (current != this)
                yield return current;

            if (current.HasChildren)
                foreach (var child in current.Children().Reverse())
                    work.Push(child);
        }
        while (work.Count != 0);
    }

    internal abstract void Visit(SemanticVisitor visitor);

    internal abstract T? Visit<T>(SemanticVisitor<T> visitor);
}
