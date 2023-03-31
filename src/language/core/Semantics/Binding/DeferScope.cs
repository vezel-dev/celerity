namespace Vezel.Celerity.Language.Semantics.Binding;

internal sealed class DeferScope : Scope, IScope<DeferScope>
{
    private DeferScope(Scope? parent)
        : base(parent)
    {
    }

    static DeferScope IScope<DeferScope>.Create(Scope? parent)
    {
        return new(parent);
    }

    public override FunctionScope? GetEnclosingFunction(bool ignoreDefer)
    {
        // Consider this code:
        //
        // err fn foo() {
        //     defer raise err MyError {};
        //     defer ret 42;
        // }
        //
        // A raise expression is allowed in a defer statement, but a ret expression is not. For the former case, we want
        // to continue the scope walk, while for the latter case, we want to short-circuit it here.
        return ignoreDefer ? Parent!.GetEnclosingFunction(ignoreDefer) : null;
    }

    public override LoopScope? GetEnclosingLoop()
    {
        // We might be analyzing code like this:
        //
        // while true {
        //     defer next;
        //
        //     raise err MyError {};
        // }
        //
        // This is erroneous; a defer statement cannot contain a break/next expression that binds to a loop outside of
        // the defer statement. So we short-circuit the scope walk by returning null here.
        return null;
    }
}
