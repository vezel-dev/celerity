using Vezel.Celerity.Semantics.Tree;

namespace Vezel.Celerity.Semantics.Binding;

internal sealed class BlockScope : Scope
{
    public ImmutableArray<UseStatementSemantics>.Builder Uses { get; } =
        ImmutableArray.CreateBuilder<UseStatementSemantics>();

    public ImmutableArray<DeferStatementSemantics>.Builder Defers { get; } =
        ImmutableArray.CreateBuilder<DeferStatementSemantics>();

    public BlockScope(Scope parent)
        : base(parent)
    {
    }

    protected override void CollectScopedStatements<T>(
        ImmutableArray<T>.Builder builder, Func<BlockScope, ImmutableArray<T>.Builder> selector, Scope? target)
    {
        var items = selector(this);

        // Consider this code:
        //
        // fn foo() {
        //     use a = create_a();
        //     use b = create_b();
        //     if something() {
        //         use c = create_c();
        //         use d = create_d();
        //         work(a, b, c, d);
        //     };
        // }
        //
        // When foo returns, all four variables must be dropped in reverse order, i.e. d, c, b, a. This is why we add
        // the statements to the builder in reverse, so we get d, c and then b, a.
        for (var i = items.Count - 1; i >= 0; i--)
            builder.Add(items[i]);

        base.CollectScopedStatements(builder, selector, target);
    }
}
