// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

public sealed class InteractiveSymbol : LocalSymbol
{
    public override bool IsMutable => _mutable;

    public override bool IsDiscard => false;

    private readonly bool _mutable;

    internal InteractiveSymbol(string name, bool mutable)
        : base(name)
    {
        _mutable = mutable;
    }

    private protected override SyntaxToken GetToken(SemanticNode node)
    {
        // We never have bindings, so this should never be called.
        throw new UnreachableException();
    }
}
