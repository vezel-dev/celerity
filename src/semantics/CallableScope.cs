namespace Vezel.Celerity.Semantics;

public abstract class CallableScope : Scope
{
    private protected CallableScope(SyntaxNode syntax, Scope parent)
        : base(syntax, parent)
    {
    }

    internal bool TryDefineSymbol(Parameter parameter, [MaybeNullWhen(true)] out Symbol existing)
    {
        return TryDefineSymbol(new ParameterSymbol(parameter, this), out existing);
    }
}
