namespace Vezel.Celerity.Language.Semantics.Binding;

internal class FunctionScope : Scope, IScope<FunctionScope>
{
    public bool IsFallible { get; set; }

    protected internal FunctionScope(Scope? parent)
        : base(parent)
    {
    }

    static FunctionScope IScope<FunctionScope>.Create(Scope? parent)
    {
        return new(parent);
    }
}
