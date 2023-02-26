using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Syntax;

public abstract partial class SyntaxVisitor<T>
{
    public T? VisitNode(SyntaxNode node)
    {
        Check.Null(node);

        return node.Visit(this);
    }

    protected virtual T? DefaultVisitNode(SyntaxNode node)
    {
        Check.Null(node);

        return default;
    }
}
