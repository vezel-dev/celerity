namespace Vezel.Celerity.Semantics;

public sealed class PatternScope : Scope
{
    public new PatternNode Syntax => Unsafe.As<PatternNode>(base.Syntax);

    public new Scope Parent => base.Parent!;

    internal PatternScope(PatternNode syntax, Scope parent)
        : base(syntax, parent)
    {
    }

    internal bool TryDefineSymbol(PatternVariableBindingNode binding, [MaybeNullWhen(true)] out Symbol existing)
    {
        return TryDefineSymbol(new VariableSymbol(binding, this), out existing);
    }
}
