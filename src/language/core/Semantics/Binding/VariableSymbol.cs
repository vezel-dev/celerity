// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

public sealed class VariableSymbol : LocalSymbol, ILocalSymbol<VariableSymbol>
{
    public override bool IsMutable => Bindings.Any(static decl => decl is VariableBindingSemantics { IsMutable: true });

    public override bool IsDiscard => Name[0] == '_';

    private VariableSymbol(string name)
        : base(name)
    {
    }

    static VariableSymbol ILocalSymbol<VariableSymbol>.Create(string name)
    {
        return new(name);
    }

    private protected override SyntaxToken GetToken(SemanticNode node)
    {
        return Unsafe.As<BindingSemantics>(node).Syntax.NameToken;
    }

    internal void AddBinding(BindingSemantics binding)
    {
        base.AddBinding(binding);
    }
}
