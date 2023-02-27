namespace Vezel.Celerity.Language.Semantics.Binding;

internal interface IScope<T>
    where T : Scope
{
    public static abstract T Create(Scope? parent);
}
