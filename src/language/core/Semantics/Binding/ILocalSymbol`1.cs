namespace Vezel.Celerity.Language.Semantics.Binding;

internal interface ILocalSymbol<T>
    where T : LocalSymbol
{
    public static abstract T Create(string name);
}
