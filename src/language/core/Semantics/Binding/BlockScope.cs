// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

internal sealed class BlockScope : Scope, IScope<BlockScope>
{
    public ImmutableArray<DeferStatementSemantics>.Builder DeferStatements { get; } =
        ImmutableArray.CreateBuilder<DeferStatementSemantics>(0);

    private BlockScope(Scope? parent)
        : base(parent)
    {
    }

    static BlockScope IScope<BlockScope>.Create(Scope? parent)
    {
        return new(parent);
    }

    protected override void CollectDefers(Scope? target, ImmutableArray<DeferStatementSemantics>.Builder builder)
    {
        // Consider this code:
        //
        // fn foo() {
        //     defer a();
        //     defer b();
        //     if something() {
        //         defer c();
        //         defer d();
        //     };
        // }
        //
        // When foo returns, all four functions must be called in reverse order, i.e. d, c, b, a. This is why we add
        // the statements to the builder in reverse, so we get d, c and then b, a.
        for (var i = DeferStatements.Count - 1; i >= 0; i--)
            builder.Add(DeferStatements[i]);

        base.CollectDefers(target, builder);
    }
}
