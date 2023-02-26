using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics;

public abstract partial class SemanticVisitor
{
    public void VisitNode(SemanticNode node)
    {
        Check.Null(node);

        node.Visit(this);
    }

    protected virtual void DefaultVisitNode(SemanticNode node)
    {
        Check.Null(node);
    }
}
