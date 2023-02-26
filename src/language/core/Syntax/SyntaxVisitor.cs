using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Syntax;

public abstract partial class SyntaxVisitor
{
    public void VisitNode(SyntaxNode node)
    {
        Check.Null(node);

        node.Visit(this);
    }

    protected virtual void DefaultVisitNode(SyntaxNode node)
    {
        Check.Null(node);
    }
}
