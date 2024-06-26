// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Syntax;

public abstract partial class SyntaxVisitor<T>
{
    public T? Visit(SyntaxNode node)
    {
        Check.Null(node);

        return node.Visit(this);
    }

    protected virtual T? DefaultVisit(SyntaxNode node)
    {
        Check.Null(node);

        return default;
    }
}
