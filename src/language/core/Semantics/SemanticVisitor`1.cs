using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics;

public abstract partial class SemanticVisitor<T>
{
    public T? VisitNode(SemanticNode node)
    {
        Check.Null(node);

        return node.Visit(this);
    }

    protected virtual T? DefaultVisitNode(SemanticNode node)
    {
        Check.Null(node);

        return default;
    }
}
