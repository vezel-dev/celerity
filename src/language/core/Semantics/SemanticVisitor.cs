using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics;

public abstract partial class SemanticVisitor
{
    public void Visit(SemanticNode node)
    {
        Check.Null(node);

        node.Visit(this);
    }

    protected virtual void DefaultVisit(SemanticNode node)
    {
        Check.Null(node);
    }
}
