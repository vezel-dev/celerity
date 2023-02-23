using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics;

public abstract partial class SemanticWalker<T>
{
    protected SemanticWalker()
    {
    }

    public T VisitNode(SemanticNode node, T state)
    {
        Check.Null(node);

        return node.Visit(this, state);
    }

    protected virtual T DefaultVisitNode(SemanticNode node, T state)
    {
        Check.Null(node);

        if (node.HasChildren)
            foreach (var child in node.Children())
                state = VisitNode(child, state);

        return state;
    }
}
