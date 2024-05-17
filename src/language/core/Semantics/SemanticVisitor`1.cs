// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics;

public abstract partial class SemanticVisitor<T>
{
    public T? Visit(SemanticNode node)
    {
        Check.Null(node);

        return node.Visit(this);
    }

    protected virtual T? DefaultVisit(SemanticNode node)
    {
        Check.Null(node);

        return default;
    }
}
