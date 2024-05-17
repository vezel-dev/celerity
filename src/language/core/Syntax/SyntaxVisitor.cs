// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Syntax;

public abstract partial class SyntaxVisitor
{
    public void Visit(SyntaxNode node)
    {
        Check.Null(node);

        node.Visit(this);
    }

    protected virtual void DefaultVisit(SyntaxNode node)
    {
        Check.Null(node);
    }
}
